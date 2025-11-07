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
        public async Task<IActionResult> CreateModuleCategoryAsync([FromBody] CreateCategoryDto categoryDto)
        {
            Result<RetrieveCategoryDto> result = await _categoryService.CreateAsync(categoryDto);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Retrieves all module categories.
        /// </summary>
        /// <response code="200">There are module categories in the system. Returns the list of
        /// RetrieveCategoryDto.</response>
        /// <response code="404">There are no module categories in the system.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is
        /// not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetAllModuleCategoriesAsync()
        {
            Result<IEnumerable<RetrieveCategoryDto>> result = await _categoryService.GetAllAsync();
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Updates an existing module category.
        /// </summary>
        /// <param name="id">The module category ID.</param>
        /// <param name="categoryDto">The updated module category data.</param>
        /// <response code="200">The module category was updated successfully. Returns the updated
        /// RetrieveCategoryDto.</response>
        /// <response code="400">Validation ERROR while validating, must be unique.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is
        /// not active or doesnt have one of the roles: Admin</response>
        /// <response code="404">The module category was not found.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPut("{id:long}")]
        [Authorize(Roles = "Admin", Policy = "ActiveUser")]
        public async Task<IActionResult> UpdateModuleCategoriesAsync(long id,[FromBody] UpdateCategoryDto categoryDto)
        {
            if (id != categoryDto.Id)
                return BadRequest("ID Missmatch");
            Result<RetrieveCategoryDto> result = await _categoryService.UpdateAsync(categoryDto);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Deletes a module category by its ID.
        /// </summary>
        /// <param name="id">The module category ID.</param>
        /// <response code="200">The module category was deleted successfully.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is
        /// not active or doesnt have one of the roles: Admin.</response>
        /// <response code="404">The module category was not found.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Admin", Policy = "ActiveUser")]
        public async Task<IActionResult> DeleteModuleCategoryAsync(long id)
        {
            Result result = await _categoryService.DeleteAsync(id);
            return _responseHandler.HandleResult(result);
        }
        
        /// <summary>
        /// Retrieves a module category by its ID.
        /// </summary>
        /// <param name="id">The module category ID.</param>
        /// <response code="200">The module category category was found. Returns the RetrieveCategoryDto.</response>
        /// <response code="404">The module category category was not found.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is
        /// not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("{id:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetModuleCategoryById(long id)
        {
            Result<RetrieveCategoryDto> result = await _categoryService.GetByIdAsync(id);
            return _responseHandler.HandleResult(result);
        }
    }
}