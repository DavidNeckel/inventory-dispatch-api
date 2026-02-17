namespace InventoryDispatchApi.Dtos;

public record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    string Sku,
    string Name,
    int Quantity
);

public record OrderResponse(
    Guid Id,
    string Status,
    DateTime CreatedAt,
    List<OrderItemResponse> Items
);
