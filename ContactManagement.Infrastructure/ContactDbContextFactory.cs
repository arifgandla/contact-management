using ContactManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ContactManagement.Infrastructure;

public sealed class ContactDbContextFactory : IDesignTimeDbContextFactory<ContactDbContext>
{
    public ContactDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ContactDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=ContactManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True",
                sql => sql.MigrationsAssembly(typeof(ContactDbContext).Assembly.GetName().Name))
            .Options;

        return new ContactDbContext(options);
    }
}
