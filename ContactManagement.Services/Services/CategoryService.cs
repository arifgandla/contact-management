using AutoMapper;
using ContactManagement.Core.DTOs.Categories;
using ContactManagement.Core.DTOs.Common;
using ContactManagement.Core.DTOs.Contacts;
using ContactManagement.Core.Entities;
using ContactManagement.Core.Exceptions;
using ContactManagement.Core.Interfaces;
using ContactManagement.Core.Interfaces.Services;
using ContactManagement.Services.Validators;
using Microsoft.Extensions.Logging;

namespace ContactManagement.Services.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CategoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<CategoryDto>> CreateAsync(
        int userId, CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        ValidationService.Validate(request);
        var name = request.CategoryName.Trim();

        if (await _unitOfWork.Categories.NameExistsForUserAsync(userId, name, null, cancellationToken))
        {
            throw new ConflictException("A category with this name already exists.");
        }

        var now = DateTime.UtcNow;
        var category = new Category
        {
            UserId = userId,
            CategoryName = name,
            Description = NullIfEmpty(request.Description),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _unitOfWork.Categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Category {CategoryId} created for user {UserId}", category.CategoryId, userId);

        return ApiResponse<CategoryDto>.Ok(_mapper.Map<CategoryDto>(category), "Category created successfully");
    }

    public async Task<ApiResponse<IReadOnlyList<CategoryDto>>> GetAllAsync(
        int userId, CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Categories.GetAllForUserAsync(userId, cancellationToken);
        return ApiResponse<IReadOnlyList<CategoryDto>>.Ok(
            _mapper.Map<IReadOnlyList<CategoryDto>>(categories), "Categories retrieved successfully");
    }

    public async Task<ApiResponse<CategoryDto>> GetByIdAsync(
        int userId, int categoryId, CancellationToken cancellationToken = default)
    {
        ValidationService.ValidateId(categoryId, "Category id");
        var category = await GetOwnedCategoryAsync(userId, categoryId, cancellationToken);
        return ApiResponse<CategoryDto>.Ok(_mapper.Map<CategoryDto>(category), "Category retrieved successfully");
    }

    public async Task<ApiResponse<CategoryDto>> UpdateAsync(
        int userId, int categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        ValidationService.ValidateId(categoryId, "Category id");
        ValidationService.Validate(request);

        var category = await GetOwnedCategoryAsync(userId, categoryId, cancellationToken);
        var name = request.CategoryName.Trim();

        if (await _unitOfWork.Categories.NameExistsForUserAsync(userId, name, categoryId, cancellationToken))
        {
            throw new ConflictException("A category with this name already exists.");
        }

        category.CategoryName = name;
        category.Description = NullIfEmpty(request.Description);
        category.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Category {CategoryId} updated by user {UserId}", categoryId, userId);

        return ApiResponse<CategoryDto>.Ok(_mapper.Map<CategoryDto>(category), "Category updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(
        int userId, int categoryId, CancellationToken cancellationToken = default)
    {
        ValidationService.ValidateId(categoryId, "Category id");
        var category = await GetOwnedCategoryAsync(userId, categoryId, cancellationToken);

        foreach (var link in category.ContactCategories.ToList())
        {
            _unitOfWork.Categories.RemoveContactCategory(link);
        }

        _unitOfWork.Categories.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Category {CategoryId} deleted by user {UserId}", categoryId, userId);

        return ApiResponse<bool>.Ok(true, "Category deleted successfully");
    }

    public async Task<ApiResponse<bool>> AssignContactAsync(
        int userId, int categoryId, int contactId, CancellationToken cancellationToken = default)
    {
        ValidationService.ValidateId(categoryId, "Category id");
        ValidationService.ValidateId(contactId, "Contact id");

        await GetOwnedCategoryAsync(userId, categoryId, cancellationToken);
        var contact = await _unitOfWork.Contacts.GetByIdForUserAsync(userId, contactId, cancellationToken);
        if (contact is null)
        {
            throw new NotFoundException($"Contact with id {contactId} was not found.");
        }

        if (await _unitOfWork.Categories.GetContactCategoryAsync(userId, categoryId, contactId, cancellationToken) is not null)
        {
            throw new ConflictException("This contact is already assigned to the category.");
        }

        await _unitOfWork.Categories.AddContactCategoryAsync(
            new ContactCategory { CategoryId = categoryId, ContactId = contactId }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Contact assigned to category successfully");
    }

    public async Task<ApiResponse<bool>> RemoveContactAsync(
        int userId, int categoryId, int contactId, CancellationToken cancellationToken = default)
    {
        ValidationService.ValidateId(categoryId, "Category id");
        ValidationService.ValidateId(contactId, "Contact id");

        var link = await _unitOfWork.Categories.GetContactCategoryAsync(userId, categoryId, contactId, cancellationToken);
        if (link is null)
        {
            throw new NotFoundException("This contact is not assigned to the category.");
        }

        _unitOfWork.Categories.RemoveContactCategory(link);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Contact removed from category successfully");
    }

    public async Task<ApiResponse<IReadOnlyList<ContactDto>>> GetContactsAsync(
        int userId, int categoryId, CancellationToken cancellationToken = default)
    {
        ValidationService.ValidateId(categoryId, "Category id");
        await GetOwnedCategoryAsync(userId, categoryId, cancellationToken);

        var contacts = await _unitOfWork.Contacts.GetByCategoryAsync(userId, categoryId, cancellationToken);
        return ApiResponse<IReadOnlyList<ContactDto>>.Ok(
            _mapper.Map<IReadOnlyList<ContactDto>>(contacts), "Category contacts retrieved successfully");
    }

    private async Task<Category> GetOwnedCategoryAsync(
        int userId, int categoryId, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdForUserAsync(userId, categoryId, cancellationToken);
        if (category is null)
        {
            throw new NotFoundException($"Category with id {categoryId} was not found.");
        }

        return category;
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
