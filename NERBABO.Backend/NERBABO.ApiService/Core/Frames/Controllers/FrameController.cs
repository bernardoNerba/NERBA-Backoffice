using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Frames.Dtos;
using NERBABO.ApiService.Core.Frames.Services;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Frames.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrameController : ControllerBase
    {
        private readonly IFrameService _frameService;
        private readonly ILogger<FrameController> _logger;
        private readonly UserManager<User> _userManager;

        public FrameController(
            IFrameService frameService,
            ILogger<FrameController> logger,
            UserManager<User> userManager)
        {
            _frameService = frameService;
            _logger = logger;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RetrieveFrameDto>>> GetAllFramesAsync()
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value ?? "");

            // Check if the user is null or if they are not an admin
            if (user == null || !await user.CheckUserHasRoleAndActive("Admin", _userManager, _logger))
            {
                return Unauthorized("Não está autorizado a aceder a esta informação.");
            }

            try
            {
                // Get all frames
                var frames = await _frameService.GetAllFramesAsync();
                if (frames.ToList().Count == 0) // No frames found
                {
                    _logger.LogInformation("No frames found while performing query.");
                    return NotFound("Não foram encontrados enquadramentos");
                }
                return Ok(frames);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving frames");
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<ActionResult> CreateFrameAsync([FromBody] CreateFrameDto frame)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value ?? "");
            // Check if the user is null or if they are not an admin
            if (user == null || !await user!.CheckUserHasRoleAndActive("Admin", _userManager, _logger))
            {
                return Unauthorized("Não está autorizado a aceder a esta informação.");
            }

            try
            {
                var newFrame = await _frameService.CreateFrameAsync(frame);
                _logger.LogInformation("Frame created successfully.");

                return Ok(new OkMessage(
                    "Enquadramento Criado.",
                    $"Foi criado um enquadramento com o programa {newFrame.Operation}.",
                    newFrame
                    ));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating frame");
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:long}")]
        public async Task<ActionResult<RetrieveFrameDto>> GetFrameAsync(long id)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value ?? "");
            // Check if the user is null or if they are not an admin
            if (user == null || !await user!.CheckUserHasRoleAndActive("Admin", _userManager, _logger))
            {
                return Unauthorized("Não está autorizado a aceder a esta informação.");
            }
            try
            {
                var frame = await _frameService.GetFrameByIdAsync(id);
                if (frame == null)
                {
                    return NotFound("Enquadramento não encontrado.");
                }
                return Ok(frame);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting single frame");
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update/{id:long}")]
        public async Task<ActionResult<RetrieveFrameDto>> UpdateFrameAsync(long id, [FromBody] UpdateFrameDto frame)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value ?? "");
            // Check if the user is null or if they are not an admin
            if (user == null || !await user!.CheckUserHasRoleAndActive("Admin", _userManager, _logger))
            {
                return Unauthorized("Não está autorizado a aceder a esta informação.");
            }

            if (id != frame.Id)
            {
                _logger.LogWarning("The id from the frame passed on the body is not the same as the one passed on the url params");
                return BadRequest("ID mismatch");
            }

            try
            {
                var updatedFrame = await _frameService.UpdateFrameAsync(frame);
                if (updatedFrame == null)
                {
                    return NotFound("Enquadramento não encontrada.");
                }

                return Ok(new OkMessage(
                    "Enquadramento Atualizada.",
                    $"Foi atualizada o enquadramento com o programa {updatedFrame.Program}.",
                    updatedFrame
                    ));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating frame");
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id:long}")]
        public async Task<ActionResult> DeleteFrameAsync(long id)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value ?? "");
            // Check if the user is null or if they are not an admin
            if (user == null || !await user!.CheckUserHasRoleAndActive("Admin", _userManager, _logger))
            {
                return Unauthorized("Não está autorizado a aceder a esta informação.");
            }

            if (id <= 0)
            {
                return BadRequest("Id de enquadramento inválido");
            }

            try
            {
                var result = await _frameService.DeleteFrameAsync(id);
                if (!result)
                {
                    return NotFound("Enquadramento não encontrado.");
                }

                return Ok(new OkMessage()
                {
                    Title = "Enquadramento Eliminado",
                    Message = $"Foi eliminado o enquadramento com o id {id}",
                    Data = result
                });
            }
            catch (Exception e)
            {
                _logger.LogError("Error while trying to delete frame: {e}", e.Message);
                return BadRequest(e.Message);
            }
        }
    }
}
