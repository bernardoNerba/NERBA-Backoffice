using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    /// <summary>
    /// Retrieves all frames.
    /// </summary>
    /// <response code="200">The are frames on the system. Returns a list of all frames.</response>
    /// <response code="404">There are no frames on the system.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user must be a active user.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpGet]
    [Authorize(Policy = "ActiveUser")]
    public async Task<IActionResult> GetAllFramesAsync()
    {
        Result<IEnumerable<RetrieveFrameDto>> frames = await _frameService.GetAllAsync();   
        return _responseHandler.HandleResult(frames);
    }

    /// <summary>
    /// Creates a new frame with logo uploads.
    /// </summary>
    /// <param name="frame">The CreateFrameDto object containing the details of the frame to be created.</param>
    /// <response code="201">Frame created successfully. Returns the created frame details.</response>
    /// <response code="400">Validation error when creating the frame, program and operation fields must be unique.</response>
    /// <response code="401">Unauthorized access. Invalid jwt. User must be an Admin.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpPost("create")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateFrameAsync([FromForm] CreateFrameDto frame)
    {
        Result<RetrieveFrameDto> result = await _frameService.CreateAsync(frame);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Retrieves a frame by its ID.
    /// </summary>
    /// <param name="id">The ID of the frame to be retrieved.</param>
    /// <response code="200">Frame found. Returns the frame details.</response>
    /// <response code="404">Frame not found.</response>
    /// <response code="401">Unauthorized access. Invalid jwt. User must be an Active user.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpGet("{id:long}")]
    [Authorize(Policy = "ActiveUser")]
    public async Task<IActionResult> GetFrameAsync(long id)
    {
        Result<RetrieveFrameDto> result = await _frameService.GetByIdAsync(id);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Updates an existing frame with optional logo uploads.
    /// </summary>
    /// <param name="id">The ID of the frame to be updated.</param>
    /// <param name="frame">The UpdateFrameDto object containing the updated details of the frame.</param>
    /// <response code="200">Frame updated successfully. Returns the updated frame details.</response>
    /// <response code="400">Validation error when updating the frame, ID mismatch or program/operation fields must be unique.</response>
    /// <response code="404">Frame not found.</response>
    /// <response code="401">Unauthorized access. Invalid jwt. User must be an Admin.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpPut("update/{id:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateFrameAsync(long id, [FromForm] UpdateFrameDto frame)
    {
        if (id != frame.Id)
            return BadRequest("ID mismatch");

        Result<RetrieveFrameDto> result = await _frameService.UpdateAsync(frame);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Deletes a frame by its ID.
    /// </summary>
    /// <param name="id">The ID of the frame to be deleted.</param>
    /// <response code="200">Frame deleted successfully.</response>
    /// <response code="404">Frame not found.</response>
    /// <response code="401">Unauthorized access. Invalid jwt. User must be an Admin.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpDelete("delete/{id:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteFrameAsync(long id)
    {
        Result result = await _frameService.DeleteAsync(id);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Retrieves the program logo for a frame.
    /// </summary>
    /// <param name="id">The ID of the frame.</param>
    /// <response code="200">Program logo retrieved successfully.</response>
    /// <response code="404">Frame or logo not found.</response>
    /// <response code="401">Unauthorized access. Invalid jwt. User must be an Active user.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpGet("{id:long}/program-logo")]
    [Authorize(Policy = "ActiveUser")]
    public async Task<IActionResult> GetProgramLogoAsync(long id)
    {
        var logoData = await _frameService.GetProgramLogoAsync(id);
        if (logoData is null)
            return NotFound("Program logo not found.");

        return File(logoData, "image/jpeg");
    }

    /// <summary>
    /// Retrieves the financement logo for a frame.
    /// </summary>
    /// <param name="id">The ID of the frame.</param>
    /// <response code="200">Financement logo retrieved successfully.</response>
    /// <response code="404">Frame or logo not found.</response>
    /// <response code="401">Unauthorized access. Invalid jwt. User must be an Active user.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpGet("{id:long}/financement-logo")]
    [Authorize(Policy = "ActiveUser")]
    public async Task<IActionResult> GetFinancementLogoAsync(long id)
    {
        var logoData = await _frameService.GetFinancementLogoAsync(id);
        if (logoData is null)
            return NotFound("Financement logo not found.");

        return File(logoData, "image/jpeg");
    }
}
