using ContactManagement.Core.DTOs.Common;
using ContactManagement.Core.DTOs.Contacts;

namespace ContactManagement.Core.Interfaces.Services;

public interface IContactService
{
    Task<ApiResponse<ContactDto>> CreateAsync(int userId, CreateContactRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<ContactDto>>> GetAllAsync(int userId, ContactQueryParams query, CancellationToken cancellationToken = default);
    Task<ApiResponse<ContactDto>> GetByIdAsync(int userId, int contactId, CancellationToken cancellationToken = default);
    Task<ApiResponse<ContactDto>> UpdateAsync(int userId, int contactId, UpdateContactRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(int userId, int contactId, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<ContactDto>>> SearchAsync(int userId, ContactQueryParams query, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<ContactDto>>> GetFavoritesAsync(int userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<ContactDto>> SetFavoriteAsync(int userId, int contactId, bool isFavorite, CancellationToken cancellationToken = default);
}
