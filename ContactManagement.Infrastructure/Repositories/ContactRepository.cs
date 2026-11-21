using ContactManagement.Core.DTOs.Common;
using ContactManagement.Core.Entities;
using ContactManagement.Core.Enums;
using ContactManagement.Core.Interfaces.Repositories;
using ContactManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ContactManagement.Infrastructure.Repositories;

internal sealed class ContactRepository : Repository<Contact>, IContactRepository
{
    public ContactRepository(ContactDbContext context) : base(context) { }

    public async Task<Contact?> GetByIdForUserAsync(
        int userId, int contactId, CancellationToken cancellationToken = default) =>
        await Context.Contacts
            .Include(c => c.ContactCategories)
                .ThenInclude(cc => cc.Category)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ContactId == contactId, cancellationToken);

    public async Task<(IReadOnlyList<Contact> Items, int TotalCount)> GetPagedAsync(
        int userId, ContactQueryParams query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var queryable = ContactsWithCategories().Where(c => c.UserId == userId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            queryable = queryable.Where(c =>
                c.FullName.ToLower().Contains(search) ||
                c.Email.ToLower().Contains(search) ||
                (c.PhoneNumber != null && c.PhoneNumber.Contains(search)) ||
                (c.Company != null && c.Company.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(query.Company))
        {
            var company = query.Company.Trim().ToLower();
            queryable = queryable.Where(c => c.Company != null && c.Company.ToLower().Contains(company));
        }

        if (query.IsFavorite.HasValue)
        {
            queryable = queryable.Where(c => c.IsFavorite == query.IsFavorite.Value);
        }

        if (query.CategoryId.HasValue)
        {
            queryable = queryable.Where(c => c.ContactCategories.Any(cc => cc.CategoryId == query.CategoryId.Value));
        }

        var totalCount = await queryable.CountAsync(cancellationToken);

        var sortBy = Enum.TryParse<ContactSortBy>(query.SortBy, true, out var parsedSort)
            ? parsedSort
            : ContactSortBy.FullName;
        var direction = Enum.TryParse<SortDirection>(query.SortDirection, true, out var parsedDirection)
            ? parsedDirection
            : SortDirection.Asc;

        IOrderedQueryable<Contact> ordered = direction == SortDirection.Asc
            ? sortBy switch
            {
                ContactSortBy.Email => queryable.OrderBy(c => c.Email),
                ContactSortBy.Company => queryable.OrderBy(c => c.Company ?? string.Empty),
                ContactSortBy.CreatedAt => queryable.OrderBy(c => c.CreatedAt),
                ContactSortBy.UpdatedAt => queryable.OrderBy(c => c.UpdatedAt),
                ContactSortBy.IsFavorite => queryable.OrderBy(c => c.IsFavorite),
                _ => queryable.OrderBy(c => c.FullName)
            }
            : sortBy switch
            {
                ContactSortBy.Email => queryable.OrderByDescending(c => c.Email),
                ContactSortBy.Company => queryable.OrderByDescending(c => c.Company ?? string.Empty),
                ContactSortBy.CreatedAt => queryable.OrderByDescending(c => c.CreatedAt),
                ContactSortBy.UpdatedAt => queryable.OrderByDescending(c => c.UpdatedAt),
                ContactSortBy.IsFavorite => queryable.OrderByDescending(c => c.IsFavorite),
                _ => queryable.OrderByDescending(c => c.FullName)
            };

        var items = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Contact>> GetByCategoryAsync(
        int userId, int categoryId, CancellationToken cancellationToken = default) =>
        await ContactsWithCategories()
            .Where(c => c.UserId == userId && c.ContactCategories.Any(cc => cc.CategoryId == categoryId))
            .OrderBy(c => c.FullName)
            .ToListAsync(cancellationToken);

    public async Task<bool> EmailExistsForUserAsync(
        int userId, string email, int? excludeContactId = null, CancellationToken cancellationToken = default)
    {
        var queryable = Context.Contacts
            .Where(c => c.UserId == userId && c.Email.ToLower() == email.ToLower());

        if (excludeContactId.HasValue)
        {
            queryable = queryable.Where(c => c.ContactId != excludeContactId.Value);
        }

        return await queryable.AnyAsync(cancellationToken);
    }

    private IQueryable<Contact> ContactsWithCategories() =>
        Context.Contacts
            .AsNoTracking()
            .Include(c => c.ContactCategories)
                .ThenInclude(cc => cc.Category);
}
