using ContactManagement.Core.DTOs.Categories;
using ContactManagement.Core.DTOs.Common;
using ContactManagement.Core.DTOs.Contacts;

namespace ContactManagement.Core.Interfaces.Services;

public interface ICategoryService
{
    Task<ApiResponse<CategoryDto>> CreateAsync(int userId, CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<CategoryDto>>> GetAllAsync(int userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CategoryDto>> GetByIdAsync(int userId, int categoryId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CategoryDto>> UpdateAsync(int userId, int categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(int userId, int categoryId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> AssignContactAsync(int userId, int categoryId, int contactId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> RemoveContactAsync(int userId, int categoryId, int contactId, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<ContactDto>>> GetContactsAsync(int userId, int categoryId, CancellationToken cancellationToken = default);
}
