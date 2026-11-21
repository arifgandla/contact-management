using ContactManagement.Core.Entities;

namespace ContactManagement.Core.Interfaces.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByIdForUserAsync(int userId, int categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAllForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> NameExistsForUserAsync(
        int userId, string categoryName, int? excludeCategoryId = null, CancellationToken cancellationToken = default);
    Task<ContactCategory?> GetContactCategoryAsync(
        int userId, int categoryId, int contactId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContactCategory>> GetContactCategoriesAsync(
        int userId, int contactId, CancellationToken cancellationToken = default);
    Task AddContactCategoryAsync(ContactCategory contactCategory, CancellationToken cancellationToken = default);
    void RemoveContactCategory(ContactCategory contactCategory);
}
