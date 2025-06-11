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

    public async Task<Result<RetrieveFrameDto>> CreateFrameAsync(CreateFrameDto frame)
    {
        if (frame is null)
        {
            _logger.LogError("CreateFrameAsync: frame is null");
            return Result<RetrieveFrameDto>
                .Fail("Não encontrado.", "Enquadramento não encontrado.", 
                StatusCodes.Status404NotFound);
        }
        if (await _context.Frames.AnyAsync(f => f.Program == frame.Program))
        {
            return Result<RetrieveFrameDto>
                .Fail("Programa duplicado.", "O Programa deve ser único. Já existe no sistema.");
        }
        if (await _context.Frames.AnyAsync(f => f.Operation == frame.Operation))
        {
            return Result<RetrieveFrameDto>
                .Fail("Operação duplicado.", "O Operação deve ser único. Já existe no sistema.");
        }

        var newFrame = Frame.ConvertCreateDtoToEntity(frame);

        _context.Frames.Add(newFrame);
        await _context.SaveChangesAsync();
        return Result<RetrieveFrameDto>
               .Ok(Frame.ConvertEntityToRetrieveDto(newFrame), "Enquadramento Criado.",
                $"Foi criado um enquadramento com o programa {newFrame.Operation}.",
                StatusCodes.Status201Created);
    }

    public async Task<Result> DeleteFrameAsync(long id)
    {
        // TODO: When dependencies are added, check if the frame can be deleted
        var existingFrame = await _context.Frames.FindAsync(id);
            
        if (existingFrame is null)
        {
            return Result
                .Fail("Não encontrado.", "Enquadramento não encontrado.", 
                StatusCodes.Status404NotFound);
        }

        _context.Remove(existingFrame);
        await _context.SaveChangesAsync();
        return Result.Ok("Enquadramento eliminado.", "Enquadramento eliminado com sucesso.");
    }

    public async Task<Result<IEnumerable<RetrieveFrameDto>>> GetAllFramesAsync()
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

    public async Task<Result<RetrieveFrameDto>> GetFrameByIdAsync(long id)
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

    public async Task<Result<RetrieveFrameDto>> UpdateFrameAsync(UpdateFrameDto frame)
    {
        var existingFrame = await _context.Frames.FindAsync(frame.Id);
        if (existingFrame is null)
        {
            return Result<RetrieveFrameDto>
                .Fail("Não encontrado.", "Enquadramento não foi encontrado.",
                StatusCodes.Status404NotFound);
        }

        if (!string.IsNullOrEmpty(frame.Program)
        && await _context.Frames.AnyAsync(f => f.Program == frame.Program && f.Id != existingFrame.Id))
        {
            return Result<RetrieveFrameDto>
                .Fail("Programa duplicado.", "O Programa deve ser único. Já existe no sistema.");
        }
        if (!string.IsNullOrEmpty(frame.Operation)
        && await _context.Frames.AnyAsync(f => f.Operation == frame.Operation && f.Id != existingFrame.Id))
        {
            return Result<RetrieveFrameDto>
                .Fail("Operação duplicado.", "O Operação deve ser único. Já existe no sistema.");
        }

        _context.Entry(existingFrame).CurrentValues.SetValues(Frame.ConvertUpdateDtoToEntity(frame));

        await _context.SaveChangesAsync();
        return Result<RetrieveFrameDto>
            .Ok(Frame.ConvertEntityToRetrieveDto(existingFrame), "Enquadramento Atualizado.",
            $"Foi atualizada o enquadramento com o programa {existingFrame.Program}.");
    }
}
