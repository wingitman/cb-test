using Infrastructure;
using Infrastructure.Helpers;
using Interview.API.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Interview.API.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/products", GetProducts).WithName("Get Products");
        endpoints.MapGet("/categories", GetCategories).WithName("Get Categories");
        endpoints.MapPut("/product/{productId:guid}/active", ToggleProductActive).WithName("Toggle Product Active");
        endpoints.MapPut("/products/{productId:guid}", UpdateProduct).WithName("Update Product");

        return endpoints;
    }

    private static async Task<IResult> GetProducts(
        InMemDbContext db,
        CancellationToken ct,
        string search = "",
        string category = "")
    {
        search = search.Trim();
        category = category.Trim();
        await Logger.DebugAsync($"Product request: Search: {search}, Category: {category}");

        var products = await db.Products
            .Join(
                db.Categories,
                p => p.CategoryId,
                c => (Guid?)c.CategoryId,
                (p, c) => new { Product = p, Category = c })
            .GroupJoin(
                db.Inventory,
                x => x.Product.Id,
                inventory => inventory.ProductId,
                (x, inventory) => new
                {
                    x.Product,
                    x.Category,
                    Inventory = inventory
                        .Join(
                            db.Locations,
                            inventory => inventory.LocationId,
                            location => location.Id,
                            (inventory, location) => new
                            {
                                inventory.Id,
                                inventory.Amount,
                                inventory.CapacityCost,
                                Location = new
                                {
                                    location.Id,
                                    location.Name,
                                    location.Region,
                                    location.Country
                                }
                            })
                        .ToList()
                })
            .Where(x =>
                string.IsNullOrEmpty(search) ||
                x.Product.Name.ToLower().Contains(search.ToLower()) ||
                x.Product.Sku.ToLower().Contains(search.ToLower()))
            .Where(x =>
                string.IsNullOrEmpty(category) ||
                x.Category.Name.ToLower().Contains(category.ToLower()))
            .OrderBy(x => x.Product.Name)
            .Select(x => new
            {
                x.Product.Id,
                x.Product.Name,
                x.Product.Sku,
                x.Product.IsActive,
                x.Product.Price,
                x.Product.Margin,
                Category = new
                {
                    Id = x.Category.CategoryId,
                    x.Category.Name
                },
                x.Inventory
            })
            .ToListAsync(ct);

        return Results.Ok(products);
    }

    private static async Task<IResult> GetCategories(InMemDbContext db, CancellationToken ct)
    {
        var categories = await db.Categories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.Name)
            .Select(category => new
            {
                Id = category.CategoryId,
                category.Name
            })
            .ToListAsync(ct);

        return Results.Ok(categories);
    }

    private static async Task<IResult> ToggleProductActive(
        Guid productId,
        InMemDbContext db,
        CancellationToken ct)
    {
        Logger.Debug($"Request to toggle {productId}'s active setting");
        var product = await db.Products.FirstOrDefaultAsync(x => x.Id == productId, ct);
        if (product == null)
            return Results.BadRequest("No such product found");

        product.IsActive = !product.IsActive;
        await db.SaveChangesAsync(ct);

        return Results.Ok(product.IsActive);
    }

    private static async Task<IResult> UpdateProduct(
        Guid productId,
        ProductRequest request,
        InMemDbContext db,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Sku) ||
            request.Price < 0)
        {
            return Results.BadRequest(new { message = "Name and SKU are required, and price cannot be negative." });
        }

        var product = await db.Products.FindAsync([productId], ct);
        if (product is null)
            return Results.NotFound(new { message = "Product was not found." });

        if (!request.CategoryId.HasValue ||
            !await db.Categories.AnyAsync(category =>
                category.CategoryId == request.CategoryId.Value && category.IsActive, ct))
        {
            return Results.BadRequest(new { message = "A valid active category is required." });
        }

        var duplicateSku = await db.Products.AnyAsync(existing =>
            existing.Id != productId && existing.Sku == request.Sku.Trim(), ct);

        if (duplicateSku)
            return Results.Conflict(new { message = "That SKU is already assigned to another product." });

        product.Name = request.Name.Trim();
        product.Sku = request.Sku.Trim();
        product.CategoryId = request.CategoryId;
        product.IsActive = request.IsActive;
        product.Price = request.Price;
        product.Margin = request.Margin;

        await db.SaveChangesAsync(ct);
        return Results.Ok(product);
    }
}
