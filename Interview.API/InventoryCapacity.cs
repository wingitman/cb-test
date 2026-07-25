using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Interview.API;

public static class InventoryCapacity
{
    public static async Task<long> GetUsedAsync(
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
