using NERBABO.ApiService.Core.Frames.Dtos;

namespace NERBABO.ApiService.Core.Frames.Services;

public interface IFrameService
{
    Task<IEnumerable<RetrieveFrameDto>> GetAllFramesAsync();
    Task<RetrieveFrameDto> GetFrameByIdAsync(long id);
    Task<RetrieveFrameDto> CreateFrameAsync(CreateFrameDto frame);
    Task<RetrieveFrameDto> UpdateFrameAsync(UpdateFrameDto frame);
    Task DeleteFrameAsync(long id);

}
