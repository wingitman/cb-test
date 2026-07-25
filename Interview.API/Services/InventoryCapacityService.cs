using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Interview.API.Services;

public static class InventoryCapacityService
{
    /// summary Calculate the used capacity at a given location. Optionally exclude inventory that is being updated
    /// param name="db"
    /// param name="locationId"
    /// param name="excludedInventoryId"
    /// param name="cancellationToken"
    public static async Task<long> CalculateCapacityAsync(
        InMemDbContext db,
        Guid locationId,
        Guid? excludedInventoryId,
        CancellationToken cancellationToken)
    {
        var inventory = await db.Inventory
            .Where(item => item.LocationId == locationId &&
                (!excludedInventoryId.HasValue || item.Id != excludedInventoryId.Value))
            .Select(item => new { item.Amount, item.CapacityCost })
            .ToListAsync(cancellationToken);

        return inventory.Sum(item => (long)item.Amount * item.CapacityCost);
    }
}
