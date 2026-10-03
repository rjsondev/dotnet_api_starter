using DotnetApiStarter.Domain.Common;

namespace DotnetApiStarter.Domain.Entities;

public class Product : BaseEntity
{
    public int Id { get; set; }

    public string Sku { get; set; } = string.Empty;

    public string? BarCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; } = string.Empty;

    public int? CategoryId { get; set; }

    public decimal? UnitPrice { get; set; }

    public int ReorderLevel { get; set; }

    public bool IsActive { get; set; }
}
