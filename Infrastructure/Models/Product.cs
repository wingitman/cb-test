namespace Infrastructure.Models;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Sku { get; set; }
    public bool IsActive { get; set; }
    public Guid? CategoryId { get; set; }
    /// Manufacturers/purchase price
    public decimal Price { get; set; }
    /// Consumer margin, inflation, etc
    public decimal Margin { get; set; }
}
