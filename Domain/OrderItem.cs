using System.ComponentModel.DataAnnotations;

namespace InventoryDispatchApi.Domain;

public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid OrderId { get; set; }

    public Order Order { get; set; } = default!;

    [Required]
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = default!;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
