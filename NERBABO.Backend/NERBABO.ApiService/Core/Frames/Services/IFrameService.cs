using NERBABO.ApiService.Core.Frames.Dtos;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Frames.Services;

public interface IFrameService
    : IGenericService<RetrieveFrameDto, CreateFrameDto, UpdateFrameDto>
{

}
