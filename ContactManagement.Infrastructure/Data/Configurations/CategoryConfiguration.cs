using ContactManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContactManagement.Infrastructure.Data.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.CategoryId);

        builder.Property(c => c.CategoryId).ValueGeneratedOnAdd();
        builder.Property(c => c.CategoryName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.CreatedAt).HasColumnType("datetime2").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnType("datetime2").IsRequired();

        builder.HasIndex(c => new { c.UserId, c.CategoryName })
            .IsUnique()
            .HasDatabaseName("IX_Categories_UserId_CategoryName");

        builder.HasOne(c => c.User)
            .WithMany(u => u.Categories)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
