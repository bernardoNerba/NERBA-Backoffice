

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Frames.Dtos;
using NERBABO.ApiService.Core.Frames.Services;
using NERBABO.ApiService.Shared.Models;
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
            var userInstance = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await Helper.AuthHelp.CheckUserHasRoleAndActive(userInstance!, "Admin", _userManager);

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
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await Helper.AuthHelp.CheckUserHasRoleAndActive(user!, "Admin", _userManager);

            var newFrame = await _frameService.CreateFrameAsync(frame);
            _logger.LogInformation("Frame created successfully.");

            return Ok(new OkMessage(
                "Enquadramento Criado.",
                $"Foi criado um enquadramento com o programa {newFrame.Operation}.",
                newFrame
            ));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:long}")]
        public async Task<ActionResult<RetrieveFrameDto>> GetFrameAsync(long id)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await Helper.AuthHelp.CheckUserHasRoleAndActive(user!, "Admin", _userManager);

            var frame = await _frameService.GetFrameByIdAsync(id)
                ?? throw new KeyNotFoundException("Enquadramento não encontrado.");
            
            return Ok(frame);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update/{id:long}")]
        public async Task<ActionResult<RetrieveFrameDto>> UpdateFrameAsync(long id, [FromBody] UpdateFrameDto frame)
        {
            if (id != frame.Id)
                return BadRequest("ID mismatch");

            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await Helper.AuthHelp.CheckUserHasRoleAndActive(user!, "Admin", _userManager);


            var updatedFrame = await _frameService.UpdateFrameAsync(frame)
                    ?? throw new KeyNotFoundException("Enquadramento não encontrado.");

            return Ok(new OkMessage(
                "Enquadramento Atualizada.",
                $"Foi atualizada o enquadramento com o programa {updatedFrame.Program}.",
                updatedFrame
            ));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id:long}")]
        public async Task<ActionResult> DeleteFrameAsync(long id)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await Helper.AuthHelp.CheckUserHasRoleAndActive(user!, "Admin", _userManager);

            await _frameService.DeleteFrameAsync(id);

            return Ok(new OkMessage()
            {
                Title = "Enquadramento Eliminado",
                Message = $"Foi eliminado o enquadramento com o id {id}",
                Data = null
            });

        }
    }
}
