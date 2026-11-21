namespace ContactManagement.Core.Entities;

public sealed class Category
{
    public int CategoryId { get; set; }
    public int UserId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<ContactCategory> ContactCategories { get; set; } = new List<ContactCategory>();
}
