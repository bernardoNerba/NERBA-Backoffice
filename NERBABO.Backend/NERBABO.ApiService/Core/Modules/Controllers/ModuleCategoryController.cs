using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Modules.Dtos;
using NERBABO.ApiService.Core.Modules.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Modules.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModuleCategoryController(
        IModuleCategoryService categoryService,
        IResponseHandler responseHandler
        ) : ControllerBase
    {
        private readonly IModuleCategoryService _categoryService = categoryService;
        private readonly IResponseHandler _responseHandler = responseHandler;
    
        /// <summary>
        /// Creates a new Module Category.
        /// </summary>
        /// <param name="categoryDto">The module category data to create.</param>
        /// <response code="201">The module category was created successfully. Returns the created
        /// RetrieveCategoryDto.</response>
        /// <response code="400">Validation ERROR while validating equality.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is
        /// not active or doesnt have one of the roles: Admin</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPost]
        [Authorize(Roles = "Admin", Policy = "ActiveUser")]
        public async Task<IActionResult> CreateModuleAsync([FromBody] CreateCategoryDto categoryDto)
        {
            Result<RetrieveCategoryDto> result = await _categoryService.CreateAsync(categoryDto);
            return _responseHandler.HandleResult(result);
        }
    }
}