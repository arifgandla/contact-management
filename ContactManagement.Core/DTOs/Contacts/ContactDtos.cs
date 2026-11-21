using System.ComponentModel.DataAnnotations;

namespace ContactManagement.Core.DTOs.Contacts;

public sealed class CreateContactRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [StringLength(50)]
    [RegularExpression(@"^\+?[0-9\s\-()]{7,50}$", ErrorMessage = "Invalid phone number format.")]
    public string? PhoneNumber { get; set; }

    [StringLength(50)]
    [RegularExpression(@"^\+?[0-9\s\-()]{7,50}$", ErrorMessage = "Invalid alternate phone number format.")]
    public string? AlternatePhoneNumber { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(200)]
    public string? Company { get; set; }

    [StringLength(200)]
    public string? JobTitle { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    public bool IsFavorite { get; set; }

    public IReadOnlyList<int> CategoryIds { get; set; } = Array.Empty<int>();
}

public sealed class UpdateContactRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [StringLength(50)]
    [RegularExpression(@"^\+?[0-9\s\-()]{7,50}$", ErrorMessage = "Invalid phone number format.")]
    public string? PhoneNumber { get; set; }

    [StringLength(50)]
    [RegularExpression(@"^\+?[0-9\s\-()]{7,50}$", ErrorMessage = "Invalid alternate phone number format.")]
    public string? AlternatePhoneNumber { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(200)]
    public string? Company { get; set; }

    [StringLength(200)]
    public string? JobTitle { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    public bool IsFavorite { get; set; }
}

public sealed class ContactDto
{
    public int ContactId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string? AlternatePhoneNumber { get; init; }
    public string? Address { get; init; }
    public string? Company { get; init; }
    public string? JobTitle { get; init; }
    public string? Notes { get; init; }
    public bool IsFavorite { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public IReadOnlyList<ContactManagement.Core.DTOs.Categories.CategoryDto> Categories { get; init; } =
        Array.Empty<ContactManagement.Core.DTOs.Categories.CategoryDto>();
}
