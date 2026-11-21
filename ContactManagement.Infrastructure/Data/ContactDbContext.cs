using ContactManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContactManagement.Infrastructure.Data;

public sealed class ContactDbContext : DbContext
{
    public ContactDbContext(DbContextOptions<ContactDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ContactCategory> ContactCategories => Set<ContactCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContactDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
