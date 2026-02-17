using System.ComponentModel.DataAnnotations;

namespace InventoryDispatchApi.Domain;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public OrderStatus Status { get; set; } = OrderStatus.Open;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = new();
}
