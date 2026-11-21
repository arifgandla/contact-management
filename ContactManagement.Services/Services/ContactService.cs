using AutoMapper;
using ContactManagement.Core.DTOs.Common;
using ContactManagement.Core.DTOs.Contacts;
using ContactManagement.Core.Entities;
using ContactManagement.Core.Exceptions;
using ContactManagement.Core.Interfaces;
using ContactManagement.Core.Interfaces.Services;
using ContactManagement.Services.Validators;
using Microsoft.Extensions.Logging;

namespace ContactManagement.Services.Services;

public sealed class ContactService : IContactService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ContactService> _logger;

    public ContactService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ContactService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<ContactDto>> CreateAsync(
        int userId, CreateContactRequest request, CancellationToken cancellationToken = default)
    {
        ValidationService.Validate(request);
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _unitOfWork.Contacts.EmailExistsForUserAsync(userId, email, null, cancellationToken))
        {
            throw new ConflictException("A contact with this email already exists.");
        }

        var now = DateTime.UtcNow;
        var contact = new Contact
        {
            UserId = userId,
            FullName = request.FullName.Trim(),
            Email = email,
            PhoneNumber = NullIfEmpty(request.PhoneNumber),
            AlternatePhoneNumber = NullIfEmpty(request.AlternatePhoneNumber),
            Address = NullIfEmpty(request.Address),
            Company = NullIfEmpty(request.Company),
            JobTitle = NullIfEmpty(request.JobTitle),
            Notes = NullIfEmpty(request.Notes),
            IsFavorite = request.IsFavorite,
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var categoryId in request.CategoryIds.Where(id => id > 0).Distinct())
        {
            var category = await _unitOfWork.Categories.GetByIdForUserAsync(userId, categoryId, cancellationToken);
            if (category is null)
            {
                throw new NotFoundException($"Category with id {categoryId} was not found.");
            }

            contact.ContactCategories.Add(new ContactCategory { CategoryId = categoryId });
        }

        await _unitOfWork.Contacts.AddAsync(contact, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _unitOfWork.Contacts.GetByIdForUserAsync(userId, contact.ContactId, cancellationToken);
        _logger.LogInformation("Contact {ContactId} created for user {UserId}", contact.ContactId, userId);

        return ApiResponse<ContactDto>.Ok(_mapper.Map<ContactDto>(created!), "Contact created successfully");
    }

    public async Task<ApiResponse<PagedResult<ContactDto>>> GetAllAsync(
        int userId, ContactQueryParams query, CancellationToken cancellationToken = default)
    {
        ValidationService.Validate(query);
        var (items, totalCount) = await _unitOfWork.Contacts.GetPagedAsync(userId, query, cancellationToken);
        return ApiResponse<PagedResult<ContactDto>>.Ok(BuildPage(items, totalCount, query), "Contacts retrieved successfully");
    }

    public async Task<ApiResponse<ContactDto>> GetByIdAsync(
        int userId, int contactId, CancellationToken cancellationToken = default)
    {
        ValidationService.ValidateId(contactId, "Contact id");
        var contact = await _unitOfWork.Contacts.GetByIdForUserAsync(userId, contactId, cancellationToken);
        if (contact is null)
        {
            throw new NotFoundException($"Contact with id {contactId} was not found.");
        }

        return ApiResponse<ContactDto>.Ok(_mapper.Map<ContactDto>(contact), "Contact retrieved successfully");
    }

    public async Task<ApiResponse<ContactDto>> UpdateAsync(
        int userId, int contactId, UpdateContactRequest request, CancellationToken cancellationToken = default)
    {
        ValidationService.ValidateId(contactId, "Contact id");
        ValidationService.Validate(request);

        var contact = await _unitOfWork.Contacts.GetByIdForUserAsync(userId, contactId, cancellationToken);
        if (contact is null)
        {
            throw new NotFoundException($"Contact with id {contactId} was not found.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _unitOfWork.Contacts.EmailExistsForUserAsync(userId, email, contactId, cancellationToken))
        {
            throw new ConflictException("A contact with this email already exists.");
        }

        contact.FullName = request.FullName.Trim();
        contact.Email = email;
        contact.PhoneNumber = NullIfEmpty(request.PhoneNumber);
        contact.AlternatePhoneNumber = NullIfEmpty(request.AlternatePhoneNumber);
        contact.Address = NullIfEmpty(request.Address);
        contact.Company = NullIfEmpty(request.Company);
        contact.JobTitle = NullIfEmpty(request.JobTitle);
        contact.Notes = NullIfEmpty(request.Notes);
        contact.IsFavorite = request.IsFavorite;
        contact.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _unitOfWork.Contacts.GetByIdForUserAsync(userId, contactId, cancellationToken);
        _logger.LogInformation("Contact {ContactId} updated by user {UserId}", contactId, userId);

        return ApiResponse<ContactDto>.Ok(_mapper.Map<ContactDto>(updated!), "Contact updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(
        int userId, int contactId, CancellationToken cancellationToken = default)
    {
        ValidationService.ValidateId(contactId, "Contact id");
        var contact = await _unitOfWork.Contacts.GetByIdForUserAsync(userId, contactId, cancellationToken);
        if (contact is null)
        {
            throw new NotFoundException($"Contact with id {contactId} was not found.");
        }

        _unitOfWork.Contacts.Remove(contact);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Contact {ContactId} deleted by user {UserId}", contactId, userId);

        return ApiResponse<bool>.Ok(true, "Contact deleted successfully");
    }

    public async Task<ApiResponse<PagedResult<ContactDto>>> SearchAsync(
        int userId, ContactQueryParams query, CancellationToken cancellationToken = default) =>
        await GetAllAsync(userId, query, cancellationToken);

    public async Task<ApiResponse<IReadOnlyList<ContactDto>>> GetFavoritesAsync(
        int userId, CancellationToken cancellationToken = default)
    {
        var query = new ContactQueryParams { IsFavorite = true, Page = 1, PageSize = 100 };
        var (items, _) = await _unitOfWork.Contacts.GetPagedAsync(userId, query, cancellationToken);
        return ApiResponse<IReadOnlyList<ContactDto>>.Ok(
            _mapper.Map<IReadOnlyList<ContactDto>>(items), "Favorite contacts retrieved successfully");
    }

    public async Task<ApiResponse<ContactDto>> SetFavoriteAsync(
        int userId, int contactId, bool isFavorite, CancellationToken cancellationToken = default)
    {
        ValidationService.ValidateId(contactId, "Contact id");
        var contact = await _unitOfWork.Contacts.GetByIdForUserAsync(userId, contactId, cancellationToken);
        if (contact is null)
        {
            throw new NotFoundException($"Contact with id {contactId} was not found.");
        }

        contact.IsFavorite = isFavorite;
        contact.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _unitOfWork.Contacts.GetByIdForUserAsync(userId, contactId, cancellationToken);
        return ApiResponse<ContactDto>.Ok(
            _mapper.Map<ContactDto>(updated!),
            isFavorite ? "Contact marked as favorite" : "Contact removed from favorites");
    }

    private PagedResult<ContactDto> BuildPage(IReadOnlyList<Contact> items, int totalCount, ContactQueryParams query) =>
        new()
        {
            Items = _mapper.Map<IReadOnlyList<ContactDto>>(items),
            Page = Math.Max(1, query.Page),
            PageSize = Math.Clamp(query.PageSize, 1, 100),
            TotalCount = totalCount
        };

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
