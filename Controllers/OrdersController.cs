using InventoryDispatchApi.Data;
using InventoryDispatchApi.Domain;
using InventoryDispatchApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryDispatchApi.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    public OrdersController(AppDbContext db) => _db = db;

    // GET /orders
    [HttpGet]
    public async Task<ActionResult<List<OrderResponse>>> GetAll(CancellationToken ct)
    {
        var orders = await _db.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderResponse(
                o.Id,
                o.Status.ToString(),
                o.CreatedAt,
                new List<OrderItemResponse>() // lista vazia no GET all (leve). Detalhe vem no GET /{id}
            ))
            .ToListAsync(ct);

        return Ok(orders);
    }

    // GET /orders/{id} (com itens + SKU)
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetById(Guid id, CancellationToken ct)
    {
        var order = await _db.Orders
            .Where(o => o.Id == id)
            .Select(o => new OrderResponse(
                o.Id,
                o.Status.ToString(),
                o.CreatedAt,
                o.Items.Select(i => new OrderItemResponse(
                    i.Id,
                    i.ProductId,
                    i.Product.Sku,
                    i.Product.Name,
                    i.Quantity
                )).ToList()
            ))
            .FirstOrDefaultAsync(ct);

        return order is null ? NotFound() : Ok(order);
    }

    // POST /orders (cria com itens)
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create([FromBody] CreateOrderRequest req, CancellationToken ct)
    {
        if (req.Items is null || req.Items.Count == 0)
            return Problem("Order precisa ter pelo menos 1 item.", statusCode: 400);

        var duplicates = req.Items
            .GroupBy(i => i.ProductId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Count > 0)
            return Problem("Não envie ProductId repetido no mesmo pedido. Consolide a quantidade.", statusCode: 400);

        var productIds = req.Items.Select(i => i.ProductId).ToList();

        var existingIds = await _db.Products
            .Where(p => productIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(ct);

        var missingProducts = productIds.Except(existingIds).ToList();
        if (missingProducts.Count > 0)
        {
            var pd = MakeProblem("Um ou mais ProductId não existem.", 400);
            pd.Extensions["missingProductIds"] = missingProducts;
            return BadRequest(pd);
        }

        var order = new Order
        {
            Status = OrderStatus.Open,
            Items = req.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList()
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        // retorna o detalhe já com SKU/Name
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, await BuildOrderResponse(order.Id, ct));
    }

    // POST /orders/{id}/dispatch
    [HttpPost("{id:guid}/dispatch")]
    public async Task<ActionResult<OrderResponse>> Dispatch(Guid id, CancellationToken ct)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (order is null) return NotFound();

        if (order.Status != OrderStatus.Open)
            return Problem("Só pode despachar quando Status = Open.", statusCode: 409);

        var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();

        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        var shortages = new List<object>();

        foreach (var item in order.Items)
        {
            var p = products[item.ProductId];

            if (p.QuantityOnHand < item.Quantity)
            {
                shortages.Add(new
                {
                    sku = p.Sku,
                    requested = item.Quantity,
                    onHand = p.QuantityOnHand,
                    missing = item.Quantity - p.QuantityOnHand
                });
            }
        }

        if (shortages.Count > 0)
        {
            var pd = MakeProblem("Estoque insuficiente para despachar.", 409);
            pd.Extensions["shortages"] = shortages;
            return Conflict(pd);
        }

        foreach (var item in order.Items)
        {
            var p = products[item.ProductId];
            p.QuantityOnHand -= item.Quantity;
        }

        order.Status = OrderStatus.Dispatched;

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Ok(await BuildOrderResponse(order.Id, ct));
    }

    private async Task<OrderResponse> BuildOrderResponse(Guid orderId, CancellationToken ct)
    {
        // busca detalhe com join em Product
        var order = await _db.Orders
            .Where(o => o.Id == orderId)
            .Select(o => new OrderResponse(
                o.Id,
                o.Status.ToString(),
                o.CreatedAt,
                o.Items.Select(i => new OrderItemResponse(
                    i.Id,
                    i.ProductId,
                    i.Product.Sku,
                    i.Product.Name,
                    i.Quantity
                )).ToList()
            ))
            .FirstAsync(ct);

        return order;
    }

    private static ProblemDetails MakeProblem(string detail, int statusCode)
        => new ProblemDetails { Detail = detail, Status = statusCode };
}
