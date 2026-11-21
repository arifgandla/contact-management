using System.ComponentModel.DataAnnotations;

namespace ContactManagement.Core.DTOs.Common;

public sealed class ContactQueryParams
{
    [StringLength(200)]
    public string? Search { get; set; }

    public bool? IsFavorite { get; set; }

    public int? CategoryId { get; set; }

    [StringLength(200)]
    public string? Company { get; set; }

    public string? SortBy { get; set; }

    public string? SortDirection { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}
