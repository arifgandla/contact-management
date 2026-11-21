using ContactManagement.Core.Interfaces.Repositories;

namespace ContactManagement.Core.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IContactRepository Contacts { get; }
    ICategoryRepository Categories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
