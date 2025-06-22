using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Frames.Dtos;
using NERBABO.ApiService.Core.Frames.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Frames.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FrameController(
        IFrameService frameService,
        IResponseHandler responseHandler
    ) : ControllerBase
{
    private readonly IFrameService _frameService = frameService;
    private readonly IResponseHandler _responseHandler = responseHandler;

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllFramesAsync()
    {
        Result<IEnumerable<RetrieveFrameDto>> frames = await _frameService.GetAllAsync();   
        return _responseHandler.HandleResult(frames);
    }

    [HttpPost("create")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateFrameAsync([FromBody] CreateFrameDto frame)
    {
        Result<RetrieveFrameDto> result = await _frameService.CreateAsync(frame);
        return _responseHandler.HandleResult(result);
    }

    [HttpGet("{id:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetFrameAsync(long id)
    {
        Result<RetrieveFrameDto> result = await _frameService.GetByIdAsync(id);
        return _responseHandler.HandleResult(result);
    }

    [HttpPut("update/{id:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateFrameAsync(long id, [FromBody] UpdateFrameDto frame)
    {
        if (id != frame.Id)
            return BadRequest("ID mismatch");

        Result<RetrieveFrameDto> result = await _frameService.UpdateAsync(frame);
        return _responseHandler.HandleResult(result);
    }

    [HttpDelete("delete/{id:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteFrameAsync(long id)
    {
        Result result = await _frameService.DeleteAsync(id);
        return _responseHandler.HandleResult(result);
    }
}
