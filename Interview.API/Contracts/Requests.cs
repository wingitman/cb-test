namespace Interview.API.Contracts;

public sealed record InventoryRequest(Guid LocationId, int Amount, int CapacityCost);

public sealed record LocationRequest(string Name, string Region, string Country, int Capacity);
