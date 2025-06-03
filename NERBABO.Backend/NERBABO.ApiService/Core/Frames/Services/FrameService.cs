using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Frames.Dtos;
using NERBABO.ApiService.Core.Frames.Models;
using NERBABO.ApiService.Data;
using ZLinq;

namespace NERBABO.ApiService.Core.Frames.Services;

public class FrameService : IFrameService
{
    private readonly AppDbContext _context;
    private readonly ILogger<FrameService> _logger;
    public FrameService(
        AppDbContext context,
        ILogger<FrameService> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<RetrieveFrameDto> CreateFrameAsync(CreateFrameDto frame)
    {
        if (frame == null)
        {
            _logger.LogError("CreateFrameAsync: frame is null");
            throw new ArgumentNullException(nameof(frame));
        }
        if (await _context.Frames.AnyAsync(f => f.Program == frame.Program))
        {
            throw new Exception("O Programa deve ser único. Já existe no sistema.");
        }
        if (await _context.Frames.AnyAsync(f => f.Operation == frame.Operation))
        {
            throw new Exception("A Operação deve ser única. Já existe no sistema.");
        }

        var newFrame = Frame.ConvertCreateDtoToEntity(frame);

        _context.Frames.Add(newFrame);
        await _context.SaveChangesAsync();
        return Frame.ConvertEntityToRetrieveDto(newFrame);
    }

    public async Task<bool> DeleteFrameAsync(long id)
    {
        // TODO: When dependencies are added, check if the frame can be deleted
        var existingFrame = await _context.Frames
            .FirstOrDefaultAsync(f => f.Id == id);

        if (existingFrame == null)
            throw new Exception("Enquadramento não foi encontrado.");

        _context.Remove(existingFrame);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<RetrieveFrameDto>> GetAllFramesAsync()
    {
        var existingFrames = await _context.Frames.ToListAsync();

        var framesToRetrieve = new List<RetrieveFrameDto>();

        foreach (var frame in existingFrames)
        {
            var frameDto = Frame.ConvertEntityToRetrieveDto(frame);
            if (frameDto != null)
                framesToRetrieve.Add(frameDto);
        }

        return [.. framesToRetrieve
                .AsValueEnumerable()
                .OrderByDescending(f => f.CreatedAt)
                .ThenBy(f => f.Program)];
    }

    public async Task<RetrieveFrameDto?> GetFrameByIdAsync(long id)
    {
        var existingFrame = await _context.Frames
            .AsNoTracking()
            .Where(f => f.Id == id)
            .FirstOrDefaultAsync();

        if (existingFrame == null)
        {
            _logger.LogWarning("GetFrameByIdAsync: Frame not found.");
            return null;
        }

        return Frame.ConvertEntityToRetrieveDto(existingFrame);
    }

    public async Task<RetrieveFrameDto?> UpdateFrameAsync(UpdateFrameDto frame)
    {
        var existingFrame = await _context.Frames
            .Where(f => f.Id == frame.Id)
            .FirstOrDefaultAsync();

        if (existingFrame == null)
        {
            _logger.LogWarning("UpdateFrameAsync: Frame not found");
            return null;
        }

        if (!string.IsNullOrEmpty(frame.Program)
        && await _context.Frames.AnyAsync(f => f.Program == frame.Program && f.Id != existingFrame.Id))
        {
            throw new Exception("O Programa deve ser único. Já existe no sistema.");
        }
        if (!string.IsNullOrEmpty(frame.Operation)
        && await _context.Frames.AnyAsync(f => f.Operation == frame.Operation && f.Id != existingFrame.Id))
        {
            throw new Exception("A Operação deve ser única. Já existe no sistema.");
        }

        _context.Entry(existingFrame).CurrentValues.SetValues(Frame.ConvertUpdateDtoToEntity(frame));

        await _context.SaveChangesAsync();
        return Frame.ConvertEntityToRetrieveDto(existingFrame);
    }
}
