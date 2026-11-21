using ContactManagement.Core.Entities;
using ContactManagement.Core.Interfaces.Repositories;
using ContactManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ContactManagement.Infrastructure.Repositories;

internal sealed class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ContactDbContext context) : base(context) { }

    public async Task<Category?> GetByIdForUserAsync(
        int userId, int categoryId, CancellationToken cancellationToken = default) =>
        await Context.Categories
            .Include(c => c.ContactCategories)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.CategoryId == categoryId, cancellationToken);

    public async Task<IReadOnlyList<Category>> GetAllForUserAsync(
        int userId, CancellationToken cancellationToken = default) =>
        await Context.Categories
            .AsNoTracking()
            .Include(c => c.ContactCategories)
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.CategoryName)
            .ToListAsync(cancellationToken);

    public async Task<bool> NameExistsForUserAsync(
        int userId, string categoryName, int? excludeCategoryId = null, CancellationToken cancellationToken = default)
    {
        var queryable = Context.Categories
            .Where(c => c.UserId == userId && c.CategoryName.ToLower() == categoryName.ToLower());

        if (excludeCategoryId.HasValue)
        {
            queryable = queryable.Where(c => c.CategoryId != excludeCategoryId.Value);
        }

        return await queryable.AnyAsync(cancellationToken);
    }

    public async Task<ContactCategory?> GetContactCategoryAsync(
        int userId, int categoryId, int contactId, CancellationToken cancellationToken = default) =>
        await Context.ContactCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(cc =>
                cc.ContactId == contactId &&
                cc.CategoryId == categoryId &&
                cc.Contact.UserId == userId &&
                cc.Category.UserId == userId,
                cancellationToken);

    public async Task<IReadOnlyList<ContactCategory>> GetContactCategoriesAsync(
        int userId, int contactId, CancellationToken cancellationToken = default) =>
        await Context.ContactCategories
            .AsNoTracking()
            .Include(cc => cc.Category)
            .Where(cc => cc.ContactId == contactId && cc.Category.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task AddContactCategoryAsync(
        ContactCategory contactCategory, CancellationToken cancellationToken = default) =>
        await Context.ContactCategories.AddAsync(contactCategory, cancellationToken);

    public void RemoveContactCategory(ContactCategory contactCategory) =>
        Context.ContactCategories.Remove(contactCategory);
}
