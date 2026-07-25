namespace Infrastructure.Models;

public class Inventory
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid LocationId { get; set; }
    public int Amount { get; set; }
    public int CapacityCost { get; set; }
}
