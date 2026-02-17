using InventoryDispatchApi.Data;
using InventoryDispatchApi.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryDispatchApi.Controllers;

[ApiController]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    public DashboardController(AppDbContext db) => _db = db;

    // GET /dashboard
    [HttpGet("/dashboard")]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var openOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Open, ct);
        var dispatchedOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Dispatched, ct);

        var belowMinimum = await _db.Products
            .Where(p => p.QuantityOnHand < p.MinimumQuantity)
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                p.Id,
                p.Sku,
                p.Name,
                p.QuantityOnHand,
                p.MinimumQuantity
            })
            .ToListAsync(ct);

        return Ok(new
        {
            openOrders,
            dispatchedOrders,
            belowMinimum
        });
    }
}
