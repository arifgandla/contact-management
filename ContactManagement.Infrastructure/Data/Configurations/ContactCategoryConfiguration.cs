using ContactManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContactManagement.Infrastructure.Data.Configurations;

internal sealed class ContactCategoryConfiguration : IEntityTypeConfiguration<ContactCategory>
{
    public void Configure(EntityTypeBuilder<ContactCategory> builder)
    {
        builder.ToTable("ContactCategories");
        builder.HasKey(cc => new { cc.ContactId, cc.CategoryId });

        builder.HasOne(cc => cc.Contact)
            .WithMany(c => c.ContactCategories)
            .HasForeignKey(cc => cc.ContactId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cc => cc.Category)
            .WithMany(c => c.ContactCategories)
            .HasForeignKey(cc => cc.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cc => cc.CategoryId).HasDatabaseName("IX_ContactCategories_CategoryId");
    }
}
