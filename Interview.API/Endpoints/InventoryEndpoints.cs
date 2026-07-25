using Infrastructure;
using Infrastructure.Models;
using Interview.API.Contracts;
using Interview.API.Services;
using Microsoft.EntityFrameworkCore;

namespace Interview.API.Endpoints;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/products/{productId:guid}/inventory", AddInventory).WithName("Add Product Inventory");
        endpoints.MapPut("/inventory/{inventoryId:guid}", UpdateInventory).WithName("Update Inventory");
        endpoints.MapDelete("/inventory/{inventoryId:guid}", DeleteInventory).WithName("Delete Inventory");

        return endpoints;
    }

    private static async Task<IResult> AddInventory(
        Guid productId,
        InventoryRequest request,
        InMemDbContext db,
        CancellationToken ct)
    {
        if (request.Amount <= 0 || request.CapacityCost <= 0)
            return Results.BadRequest(new { message = "Amount and capacity cost must be greater than zero." });

        if (!await db.Products.AnyAsync(product => product.Id == productId, ct))
            return Results.NotFound(new { message = "Product was not found." });

        var location = await db.Locations.FindAsync([request.LocationId], ct);
        if (location is null)
            return Results.NotFound(new { message = "Location was not found." });

        if (await db.Inventory.AnyAsync(item => item.ProductId == productId && item.LocationId == request.LocationId, ct))
            return Results.Conflict(new { message = "This product already has inventory at that location. Edit the existing record instead." });

        var usedCapacity = await InventoryCapacityService.CalculateCapacityAsync(db, location.Id, null, ct);
        var requestedCapacity = (long)request.Amount * request.CapacityCost;

        if (usedCapacity + requestedCapacity > location.Capacity)
            return Results.Conflict(new { message = $"The inventory would exceed {location.Name}'s capacity by {usedCapacity + requestedCapacity - location.Capacity}." });

        var inventory = new Inventory
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            LocationId = request.LocationId,
            Amount = request.Amount,
            CapacityCost = request.CapacityCost
        };

        db.Inventory.Add(inventory);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/inventory/{inventory.Id}", inventory);
    }

    private static async Task<IResult> UpdateInventory(
        Guid inventoryId,
        InventoryRequest request,
        InMemDbContext db,
        CancellationToken ct)
    {
        if (request.Amount <= 0 || request.CapacityCost <= 0)
            return Results.BadRequest(new { message = "Amount and capacity cost must be greater than zero." });

        var inventory = await db.Inventory.FindAsync([inventoryId], ct);
        if (inventory is null)
            return Results.NotFound(new { message = "Inventory was not found." });

        var location = await db.Locations.FindAsync([request.LocationId], ct);
        if (location is null)
            return Results.NotFound(new { message = "Location was not found." });

        var duplicate = await db.Inventory.AnyAsync(item =>
            item.Id != inventoryId &&
            item.ProductId == inventory.ProductId &&
            item.LocationId == request.LocationId, ct);

        if (duplicate)
            return Results.Conflict(new { message = "This product already has inventory at that location." });

        var usedCapacity = await InventoryCapacityService.CalculateCapacityAsync(db, location.Id, inventoryId, ct);
        var requestedCapacity = (long)request.Amount * request.CapacityCost;

        if (usedCapacity + requestedCapacity > location.Capacity)
            return Results.Conflict(new { message = $"The inventory would exceed {location.Name}'s capacity by {usedCapacity + requestedCapacity - location.Capacity}." });

        inventory.LocationId = request.LocationId;
        inventory.Amount = request.Amount;
        inventory.CapacityCost = request.CapacityCost;
        await db.SaveChangesAsync(ct);

        return Results.Ok(inventory);
    }

    private static async Task<IResult> DeleteInventory(
        Guid inventoryId,
        InMemDbContext db,
        CancellationToken ct)
    {
        var inventory = await db.Inventory.FindAsync([inventoryId], ct);
        if (inventory is null)
            return Results.NotFound(new { message = "Inventory was not found." });

        db.Inventory.Remove(inventory);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }
}
