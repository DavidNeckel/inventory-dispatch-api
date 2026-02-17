using System.ComponentModel.DataAnnotations;

namespace InventoryDispatchApi.Domain;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(40)]
    public string Sku { get; set; } = default!;

    [Required]
    [MaxLength(140)]
    public string Name { get; set; } = default!;

    [Range(0, int.MaxValue)]
    public int QuantityOnHand { get; set; }

    [Range(0, int.MaxValue)]
    public int MinimumQuantity { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
