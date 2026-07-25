using Infrastructure;
using Infrastructure.Models;
using Interview.API.Contracts;
using Interview.API.Services;
using Microsoft.EntityFrameworkCore;

namespace Interview.API.Endpoints;

public static class LocationEndpoints
{
    public static IEndpointRouteBuilder MapLocationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/locations", GetLocations).WithName("Get Locations");
        endpoints.MapPost("/locations", AddLocation).WithName("Add Location");
        endpoints.MapPut("/locations/{locationId:guid}", UpdateLocation).WithName("Update Location");
        endpoints.MapDelete("/locations/{locationId:guid}", DeleteLocation).WithName("Delete Location");

        return endpoints;
    }

    private static async Task<IResult> GetLocations(InMemDbContext db, CancellationToken ct)
    {
        var locations = await db.Locations
            .AsNoTracking()
            .OrderBy(location => location.Name)
            .ToListAsync(ct);

        var inventory = await db.Inventory
            .AsNoTracking()
            .Select(item => new { item.LocationId, item.Amount, item.CapacityCost })
            .ToListAsync(ct);

        var result = locations.Select(location =>
        {
            var usedCapacity = inventory
                .Where(item => item.LocationId == location.Id)
                .Sum(item => (long)item.Amount * item.CapacityCost);

            return new
            {
                location.Id,
                location.Name,
                location.Region,
                location.Country,
                location.Capacity,
                UsedCapacity = usedCapacity,
                RemainingCapacity = location.Capacity - usedCapacity
            };
        });

        return Results.Ok(result);
    }

    private static async Task<IResult> AddLocation(
        LocationRequest request,
        InMemDbContext db,
        CancellationToken ct)
    {
        if (!IsValid(request))
            return Results.BadRequest(new { message = "Location details are required and capacity must be greater than zero." });

        var location = new Location
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Region = request.Region.Trim(),
            Country = request.Country.Trim(),
            Capacity = request.Capacity
        };

        db.Locations.Add(location);
        await db.SaveChangesAsync(ct);
        return Results.Created($"/locations/{location.Id}", location);
    }

    private static async Task<IResult> UpdateLocation(
        Guid locationId,
        LocationRequest request,
        InMemDbContext db,
        CancellationToken ct)
    {
        if (!IsValid(request))
            return Results.BadRequest(new { message = "Location details are required and capacity must be greater than zero." });

        var location = await db.Locations.FindAsync([locationId], ct);
        if (location is null)
            return Results.NotFound(new { message = "Location was not found." });

        var usedCapacity = await InventoryCapacityService.CalculateCapacityAsync(db, locationId, null, ct);
        if (request.Capacity < usedCapacity)
            return Results.Conflict(new { message = $"Capacity cannot be lower than the current usage of {usedCapacity}." });

        location.Name = request.Name.Trim();
        location.Region = request.Region.Trim();
        location.Country = request.Country.Trim();
        location.Capacity = request.Capacity;
        await db.SaveChangesAsync(ct);

        return Results.Ok(location);
    }

    private static async Task<IResult> DeleteLocation(
        Guid locationId,
        InMemDbContext db,
        CancellationToken ct)
    {
        var location = await db.Locations.FindAsync([locationId], ct);
        if (location is null)
            return Results.NotFound(new { message = "Location was not found." });

        if (await db.Inventory.AnyAsync(item => item.LocationId == locationId, ct))
            return Results.Conflict(new { message = "Move all product inventory out of this location before removing it." });

        db.Locations.Remove(location);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static bool IsValid(LocationRequest request) =>
        !string.IsNullOrWhiteSpace(request.Name) &&
        !string.IsNullOrWhiteSpace(request.Region) &&
        !string.IsNullOrWhiteSpace(request.Country) &&
        request.Capacity > 0;
}
