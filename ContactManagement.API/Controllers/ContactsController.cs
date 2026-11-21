using ContactManagement.API.Extensions;
using ContactManagement.Core.DTOs.Common;
using ContactManagement.Core.DTOs.Contacts;
using ContactManagement.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactManagement.API.Controllers;

[Authorize]
public sealed class ContactsController : ApiControllerBase
{
    private readonly IContactService _contactService;

    public ContactsController(IContactService contactService) => _contactService = contactService;

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ContactDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateContactRequest request, CancellationToken cancellationToken) =>
        Respond(await _contactService.CreateAsync(User.GetUserId(), request, cancellationToken));

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ContactDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] ContactQueryParams query, CancellationToken cancellationToken) =>
        Respond(await _contactService.GetAllAsync(User.GetUserId(), query, cancellationToken));

    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ContactDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] ContactQueryParams query, CancellationToken cancellationToken) =>
        Respond(await _contactService.SearchAsync(User.GetUserId(), query, cancellationToken));

    [HttpGet("favorites")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ContactDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFavorites(CancellationToken cancellationToken) =>
        Respond(await _contactService.GetFavoritesAsync(User.GetUserId(), cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ContactDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
        Respond(await _contactService.GetByIdAsync(User.GetUserId(), id, cancellationToken));

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ContactDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateContactRequest request, CancellationToken cancellationToken) =>
        Respond(await _contactService.UpdateAsync(User.GetUserId(), id, request, cancellationToken));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        Respond(await _contactService.DeleteAsync(User.GetUserId(), id, cancellationToken));

    [HttpPatch("{id:int}/favorite")]
    [ProducesResponseType(typeof(ApiResponse<ContactDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetFavorite(
        int id, [FromQuery] bool isFavorite, CancellationToken cancellationToken) =>
        Respond(await _contactService.SetFavoriteAsync(User.GetUserId(), id, isFavorite, cancellationToken));
}
