using System.ComponentModel.DataAnnotations;

namespace InventoryDispatchApi.Dtos;

public record CreateProductRequest(
    [Required, MaxLength(40)] string Sku,
    [Required, MaxLength(140)] string Name,
    [Range(0, int.MaxValue)] int QuantityOnHand,
    [Range(0, int.MaxValue)] int MinimumQuantity
);

public record UpdateProductRequest(
    [Required, MaxLength(40)] string Sku,
    [Required, MaxLength(140)] string Name,
    [Range(0, int.MaxValue)] int QuantityOnHand,
    [Range(0, int.MaxValue)] int MinimumQuantity
);
