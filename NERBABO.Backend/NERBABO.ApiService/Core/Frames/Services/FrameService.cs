using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Frames.Dtos;
using NERBABO.ApiService.Core.Frames.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;
using ZLinq;

namespace NERBABO.ApiService.Core.Frames.Services;

public class FrameService(
    AppDbContext context,
    ILogger<FrameService> logger
    ) : IFrameService
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<FrameService> _logger = logger;

    public async Task<Result<RetrieveFrameDto>> CreateAsync(CreateFrameDto entityDto)
    {
        if (await _context.Frames.AnyAsync(f => f.Program == entityDto.Program))
        {
            return Result<RetrieveFrameDto>
                .Fail("Erro de Validação.", "O Programa deve ser único. Já existe no sistema.");
        }
        if (await _context.Frames.AnyAsync(f => f.Operation == entityDto.Operation))
        {
            return Result<RetrieveFrameDto>
                .Fail("Erro de Validação.", "O Operação deve ser único. Já existe no sistema.");
        }

        var newFrame = Frame.ConvertCreateDtoToEntity(entityDto);

        _context.Frames.Add(newFrame);
        await _context.SaveChangesAsync();
        return Result<RetrieveFrameDto>
               .Ok(Frame.ConvertEntityToRetrieveDto(newFrame), "Enquadramento Criado.",
                $"Foi criado um enquadramento com o programa {newFrame.Operation}.",
                StatusCodes.Status201Created);
    }

    public async Task<Result> DeleteAsync(long id)
    {
        var existingFrame = await _context.Frames.FindAsync(id);
            
        if (existingFrame is null)
        {
            _logger.LogWarning("Frame not found");
            return Result
                .Fail("Não encontrado.", "Enquadramento não encontrado.", 
                StatusCodes.Status404NotFound);
        }

        if(await _context.Courses.Where(c => c.FrameId == id).AnyAsync())
        {
            _logger.LogWarning("Cannot delete a frame that is associated with courses.");
            return Result
                .Fail("Não é possível eliminar.", "O enquadramento está associado a cursos.");
        }

        _context.Remove(existingFrame);
        await _context.SaveChangesAsync();
        return Result.Ok("Enquadramento eliminado.", "Enquadramento eliminado com sucesso.");
    }

    public async Task<Result<IEnumerable<RetrieveFrameDto>>> GetAllAsync()
    {
        var existingFrames = await _context.Frames
            .OrderByDescending(f => f.CreatedAt)
            .ThenBy(f => f.Program)
            .Select(f => Frame.ConvertEntityToRetrieveDto(f))
            .ToListAsync();


        if (existingFrames is null || existingFrames.Count == 0)
        {
            return Result<IEnumerable<RetrieveFrameDto>>
                .Fail("Não encontrado.", "Não foram encontrados enquadramentos",
                StatusCodes.Status404NotFound);
        }

        return Result<IEnumerable<RetrieveFrameDto>>
            .Ok(existingFrames);
    }

    public async Task<Result<RetrieveFrameDto>> GetByIdAsync(long id)
    {
        var existingFrame = await _context.Frames.FindAsync(id);

        if (existingFrame is null)
        {
            return Result<RetrieveFrameDto>
                .Fail("Não encontrado.", "Enquadramento não encontrado",
                StatusCodes.Status404NotFound);
        }

        return Result<RetrieveFrameDto>
            .Ok(Frame.ConvertEntityToRetrieveDto(existingFrame));
    }

    public async Task<Result<RetrieveFrameDto>> UpdateAsync(UpdateFrameDto entityDto)
    {
        var existingFrame = await _context.Frames.FindAsync(entityDto.Id);
        if (existingFrame is null)
        {
            return Result<RetrieveFrameDto>
                .Fail("Não encontrado.", "Enquadramento não foi encontrado.",
                StatusCodes.Status404NotFound);
        }

        if (!string.IsNullOrEmpty(entityDto.Program)
        && await _context.Frames.AnyAsync(f => f.Program == entityDto.Program && f.Id != existingFrame.Id))
        {
            return Result<RetrieveFrameDto>
                .Fail("Programa duplicado.", "O Programa deve ser único. Já existe no sistema.");
        }
        if (!string.IsNullOrEmpty(entityDto.Operation)
        && await _context.Frames.AnyAsync(f => f.Operation == entityDto.Operation && f.Id != existingFrame.Id))
        {
            return Result<RetrieveFrameDto>
                .Fail("Operação duplicado.", "O Operação deve ser único. Já existe no sistema.");
        }

        _context.Entry(existingFrame).CurrentValues.SetValues(Frame.ConvertUpdateDtoToEntity(entityDto));

        await _context.SaveChangesAsync();
        return Result<RetrieveFrameDto>
            .Ok(Frame.ConvertEntityToRetrieveDto(existingFrame), "Enquadramento Atualizado.",
            $"Foi atualizada o enquadramento com o programa {existingFrame.Program}.");
    }
}
