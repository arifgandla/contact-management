using ContactManagement.API.Extensions;
using ContactManagement.Core.DTOs.Categories;
using ContactManagement.Core.DTOs.Common;
using ContactManagement.Core.DTOs.Contacts;
using ContactManagement.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactManagement.API.Controllers;

[Authorize]
public sealed class CategoriesController : ApiControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService) => _categoryService = categoryService;

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryRequest request, CancellationToken cancellationToken) =>
        Respond(await _categoryService.CreateAsync(User.GetUserId(), request, cancellationToken));

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Respond(await _categoryService.GetAllAsync(User.GetUserId(), cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
        Respond(await _categoryService.GetByIdAsync(User.GetUserId(), id, cancellationToken));

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken) =>
        Respond(await _categoryService.UpdateAsync(User.GetUserId(), id, request, cancellationToken));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        Respond(await _categoryService.DeleteAsync(User.GetUserId(), id, cancellationToken));

    [HttpPost("{categoryId:int}/contacts/{contactId:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignContact(
        int categoryId, int contactId, CancellationToken cancellationToken) =>
        Respond(await _categoryService.AssignContactAsync(User.GetUserId(), categoryId, contactId, cancellationToken));

    [HttpDelete("{categoryId:int}/contacts/{contactId:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveContact(
        int categoryId, int contactId, CancellationToken cancellationToken) =>
        Respond(await _categoryService.RemoveContactAsync(User.GetUserId(), categoryId, contactId, cancellationToken));

    [HttpGet("{categoryId:int}/contacts")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ContactDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContacts(int categoryId, CancellationToken cancellationToken) =>
        Respond(await _categoryService.GetContactsAsync(User.GetUserId(), categoryId, cancellationToken));
}
