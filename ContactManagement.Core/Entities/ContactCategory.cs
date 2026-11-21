namespace ContactManagement.Core.Entities;

public sealed class ContactCategory
{
    public int ContactId { get; set; }
    public int CategoryId { get; set; }

    public Contact Contact { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
