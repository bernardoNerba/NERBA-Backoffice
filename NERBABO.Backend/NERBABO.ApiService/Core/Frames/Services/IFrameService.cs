using NERBABO.ApiService.Core.Frames.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Frames.Services;

public interface IFrameService
{
    Task<Result<IEnumerable<RetrieveFrameDto>>> GetAllFramesAsync();
    Task<Result<RetrieveFrameDto>> GetFrameByIdAsync(long id);
    Task<Result<RetrieveFrameDto>> CreateFrameAsync(CreateFrameDto frame);
    Task<Result<RetrieveFrameDto>> UpdateFrameAsync(UpdateFrameDto frame);
    Task<Result> DeleteFrameAsync(long id);

}
