using ContactManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContactManagement.Infrastructure.Data.Configurations;

internal sealed class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contacts");
        builder.HasKey(c => c.ContactId);

        builder.Property(c => c.ContactId).ValueGeneratedOnAdd();
        builder.Property(c => c.FullName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(256);
        builder.Property(c => c.PhoneNumber).HasMaxLength(50);
        builder.Property(c => c.AlternatePhoneNumber).HasMaxLength(50);
        builder.Property(c => c.Address).HasMaxLength(500);
        builder.Property(c => c.Company).HasMaxLength(200);
        builder.Property(c => c.JobTitle).HasMaxLength(200);
        builder.Property(c => c.Notes).HasMaxLength(2000);
        builder.Property(c => c.IsFavorite).IsRequired().HasDefaultValue(false);
        builder.Property(c => c.CreatedAt).HasColumnType("datetime2").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnType("datetime2").IsRequired();

        builder.HasIndex(c => new { c.UserId, c.Email })
            .IsUnique()
            .HasDatabaseName("IX_Contacts_UserId_Email");
        builder.HasIndex(c => new { c.UserId, c.FullName }).HasDatabaseName("IX_Contacts_UserId_FullName");
        builder.HasIndex(c => new { c.UserId, c.Company }).HasDatabaseName("IX_Contacts_UserId_Company");
        builder.HasIndex(c => new { c.UserId, c.IsFavorite }).HasDatabaseName("IX_Contacts_UserId_IsFavorite");

        builder.HasOne(c => c.User)
            .WithMany(u => u.Contacts)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
