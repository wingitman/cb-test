namespace Infrastructure.Models;

public class Location
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Region { get; set; }
    public string Country { get; set; }
    public int Capacity { get; set; }
}
