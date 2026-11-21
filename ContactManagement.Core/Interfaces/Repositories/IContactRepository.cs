using ContactManagement.Core.DTOs.Common;
using ContactManagement.Core.Entities;

namespace ContactManagement.Core.Interfaces.Repositories;

public interface IContactRepository : IRepository<Contact>
{
    Task<Contact?> GetByIdForUserAsync(int userId, int contactId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Contact> Items, int TotalCount)> GetPagedAsync(
        int userId, ContactQueryParams query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Contact>> GetByCategoryAsync(
        int userId, int categoryId, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsForUserAsync(
        int userId, string email, int? excludeContactId = null, CancellationToken cancellationToken = default);
}
