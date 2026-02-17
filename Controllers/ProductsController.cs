using InventoryDispatchApi.Data;
using InventoryDispatchApi.Domain;
using InventoryDispatchApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryDispatchApi.Controllers;

[ApiController]
[Route("products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProductsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetAll(CancellationToken ct)
    {
        var products = await _db.Products.OrderBy(p => p.Name).ToListAsync(ct);
        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Product>> GetById(Guid id, CancellationToken ct)
    {
        var product = await _db.Products.FindAsync(new object?[] { id }, ct);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create([FromBody] CreateProductRequest req, CancellationToken ct)
    {
        var sku = req.Sku.Trim();
        var exists = await _db.Products.AnyAsync(p => p.Sku == sku, ct);
        if (exists)
            return Problem("Sku já existe.", statusCode: 409);

        var product = new Product
        {
            Sku = sku,
            Name = req.Name.Trim(),
            QuantityOnHand = req.QuantityOnHand,
            MinimumQuantity = req.MinimumQuantity
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Product>> Update(Guid id, [FromBody] UpdateProductRequest req, CancellationToken ct)
    {
        var product = await _db.Products.FindAsync(new object?[] { id }, ct);
        if (product is null) return NotFound();

        var sku = req.Sku.Trim();

        var skuUsedByOther = await _db.Products.AnyAsync(p => p.Sku == sku && p.Id != id, ct);
        if (skuUsedByOther)
            return Problem("Sku já existe.", statusCode: 409);

        product.Sku = sku;
        product.Name = req.Name.Trim();
        product.QuantityOnHand = req.QuantityOnHand;
        product.MinimumQuantity = req.MinimumQuantity;

        await _db.SaveChangesAsync(ct);
        return Ok(product);
    }
}
