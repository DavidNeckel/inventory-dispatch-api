using System.ComponentModel.DataAnnotations;

namespace InventoryDispatchApi.Dtos;

public record CreateOrderItemRequest(
    [Required] Guid ProductId,
    [Range(1, int.MaxValue)] int Quantity
);

public class CreateOrderRequest
{
    [Required, MinLength(1)]
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}
