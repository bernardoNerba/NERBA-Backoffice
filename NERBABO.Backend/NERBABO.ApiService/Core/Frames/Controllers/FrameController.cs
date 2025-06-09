using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Frames.Dtos;
using NERBABO.ApiService.Core.Frames.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;
using System.Security.Claims;

namespace NERBABO.ApiService.Core.Frames.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrameController : ControllerBase
    {
        private readonly IFrameService _frameService;
        private readonly ILogger<FrameController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IResponseHandler _responseHandler;

        public FrameController(
            IFrameService frameService,
            ILogger<FrameController> logger,
            UserManager<User> userManager,
            IResponseHandler responseHandler)
        {
            _frameService = frameService;
            _logger = logger;
            _userManager = userManager;
            _responseHandler = responseHandler;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllFramesAsync()
        {
            // Get the user from the token
            var userInstance = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await Helper.AuthHelp.CheckUserHasRoleAndActive(userInstance!, "Admin", _userManager);

            // Get all frames
            Result<IEnumerable<RetrieveFrameDto>> frames = await _frameService.GetAllFramesAsync();
            
            return _responseHandler.HandleResult(frames);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateFrameAsync([FromBody] CreateFrameDto frame)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await Helper.AuthHelp.CheckUserHasRoleAndActive(user!, "Admin", _userManager);

            Result<RetrieveFrameDto> result = await _frameService.CreateFrameAsync(frame);

            return _responseHandler.HandleResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetFrameAsync(long id)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await Helper.AuthHelp.CheckUserHasRoleAndActive(user!, "Admin", _userManager);

            Result<RetrieveFrameDto> result = await _frameService.GetFrameByIdAsync(id);
            return _responseHandler.HandleResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update/{id:long}")]
        public async Task<IActionResult> UpdateFrameAsync(long id, [FromBody] UpdateFrameDto frame)
        {
            if (id != frame.Id)
                return BadRequest("ID mismatch");

            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await Helper.AuthHelp.CheckUserHasRoleAndActive(user!, "Admin", _userManager);


            Result<RetrieveFrameDto> result = await _frameService.UpdateFrameAsync(frame);

            return _responseHandler.HandleResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id:long}")]
        public async Task<IActionResult> DeleteFrameAsync(long id)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await Helper.AuthHelp.CheckUserHasRoleAndActive(user!, "Admin", _userManager);

            Result result = await _frameService.DeleteFrameAsync(id);

            return _responseHandler.HandleResult(result);

        }
    }
}
