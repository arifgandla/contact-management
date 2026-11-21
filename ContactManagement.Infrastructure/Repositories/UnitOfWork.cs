using ContactManagement.Core.Interfaces;
using ContactManagement.Core.Interfaces.Repositories;
using ContactManagement.Infrastructure.Data;

namespace ContactManagement.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ContactDbContext _context;
    private IUserRepository? _users;
    private IContactRepository? _contacts;
    private ICategoryRepository? _categories;

    public UnitOfWork(ContactDbContext context) => _context = context;

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IContactRepository Contacts => _contacts ??= new ContactRepository(_context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
