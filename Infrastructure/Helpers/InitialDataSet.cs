using Infrastructure.Models;

namespace Infrastructure.Helpers;

public static class InitialDataSet
{
    public static void Seed(InMemDbContext db)
    {
        if (db.Products.Any())
            return;

        var categories = new[]
        {
            new Category { CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "GPU", IsActive = true },
            new Category { CategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "CPU", IsActive = true },
            new Category { CategoryId = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Motherboard", IsActive = true },
            new Category { CategoryId = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "RAM", IsActive = true },
            new Category { CategoryId = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "Storage", IsActive = true },
            new Category { CategoryId = Guid.Parse("66666666-6666-6666-6666-666666666666"), Name = "Mouse", IsActive = true },
            new Category { CategoryId = Guid.Parse("77777777-7777-7777-7777-777777777777"), Name = "Keyboard", IsActive = true },
            new Category { CategoryId = Guid.Parse("88888888-8888-8888-8888-888888888888"), Name = "Monitor", IsActive = true }
        };

        var locations = new[]
        {
            new Location
            {
                Id = Guid.NewGuid(),
                Name = "Headquarters",
                Region = "North West",
                Country = "United Kingdom",
                Capacity = 1000
            },
            new Location
            {
                Id = Guid.NewGuid(),
                Name = "London Branch",
                Region = "South",
                Country = "United Kingdom",
                Capacity = 500
            },
            new Location
            {
                Id = Guid.NewGuid(),
                Name = "Ireland",
                Region = "South",
                Country = "Ireland",
                Capacity = 500
            }
        };

        var products = new[]
        {
            new Product { Id = Guid.NewGuid(), Name = "NVIDIA GeForce RTX 4070", Sku = "GPU-4070-NV", IsActive = true, CategoryId = categories[0].CategoryId, Price = 499.59m, Margin = 50m },
            new Product { Id = Guid.NewGuid(), Name = "NVIDIA GeForce RTX 4080", Sku = "GPU-4080-NV", IsActive = true, CategoryId = categories[0].CategoryId, Price = 999m, Margin = 100m },
            new Product { Id = Guid.NewGuid(), Name = "NVIDIA GeForce RTX 4090", Sku = "GPU-4090-NV", IsActive = false, CategoryId = categories[0].CategoryId, Price = 1599m, Margin = 150m },
            new Product { Id = Guid.NewGuid(), Name = "AMD Radeon RX 7800 XT", Sku = "GPU-7800-AMD", IsActive = true, CategoryId = categories[0].CategoryId, Price = 449m, Margin = 50m },
            new Product { Id = Guid.NewGuid(), Name = "AMD Radeon RX 7900 XT", Sku = "GPU-7900-AMD", IsActive = true, CategoryId = categories[0].CategoryId, Price = 699m, Margin = 80m },

            new Product { Id = Guid.NewGuid(), Name = "Intel Core i9-13900K", Sku = "CPU-I9-13900K", IsActive = true, CategoryId = categories[1].CategoryId, Price = 499m, Margin = 50m },
            new Product { Id = Guid.NewGuid(), Name = "Intel Core i7-13700K", Sku = "CPU-I7-13700K", IsActive = true, CategoryId = categories[1].CategoryId, Price = 349m, Margin = 40m },
            new Product { Id = Guid.NewGuid(), Name = "AMD Ryzen 9 7950X", Sku = "CPU-R9-7950X", IsActive = true, CategoryId = categories[1].CategoryId, Price = 549m, Margin = 50m },
            new Product { Id = Guid.NewGuid(), Name = "AMD Ryzen 7 7800X3D", Sku = "CPU-R7-7800X3D", IsActive = false, CategoryId = categories[1].CategoryId, Price = 399m, Margin = 40m },

            new Product { Id = Guid.NewGuid(), Name = "ASUS ROG Strix Z790-E", Sku = "MB-Z790E-ASUS", IsActive = true, CategoryId = categories[2].CategoryId, Price = 299m, Margin = 30m },
            new Product { Id = Guid.NewGuid(), Name = "MSI B650 Tomahawk", Sku = "MB-B650-MSI", IsActive = true, CategoryId = categories[2].CategoryId, Price = 179m, Margin = 20m },
            new Product { Id = Guid.NewGuid(), Name = "Gigabyte X670 Aorus Elite", Sku = "MB-X670-GIGA", IsActive = true, CategoryId = categories[2].CategoryId, Price = 249m, Margin = 30m },

            new Product { Id = Guid.NewGuid(), Name = "Corsair Vengeance DDR5 32GB", Sku = "RAM-32G-DDR5-COR", IsActive = true, CategoryId = categories[3].CategoryId, Price = 109m, Margin = 20m },
            new Product { Id = Guid.NewGuid(), Name = "G.Skill Trident Z5 RGB 64GB", Sku = "RAM-64G-Z5-GSK", IsActive = false, CategoryId = categories[3].CategoryId, Price = 199m, Margin = 30m },

            new Product { Id = Guid.NewGuid(), Name = "Samsung 990 Pro 2TB SSD", Sku = "SSD-2TB-990PRO", IsActive = true, CategoryId = categories[4].CategoryId, Price = 169m, Margin = 20m },
            new Product { Id = Guid.NewGuid(), Name = "WD Black SN850X 1TB SSD", Sku = "SSD-1TB-SN850X", IsActive = true, CategoryId = categories[4].CategoryId, Price = 89m, Margin = 10m },

            new Product { Id = Guid.NewGuid(), Name = "Logitech MX Master 3 Mouse", Sku = "PER-MX3-LOGI", IsActive = true, CategoryId = categories[5].CategoryId, Price = 79m, Margin = 10m },
            new Product { Id = Guid.NewGuid(), Name = "Razer Huntsman Keyboard", Sku = "PER-HUNT-RAZER", IsActive = false, CategoryId = categories[6].CategoryId, Price = 149m, Margin = 20m },
            new Product { Id = Guid.NewGuid(), Name = "Dell UltraSharp 27 Monitor", Sku = "MON-27-UDEL", IsActive = true, CategoryId = categories[7].CategoryId, Price = 299m, Margin = 30m }
        };

        var inventory = new[]
        {
            new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = products.Single(p => p.Sku == "GPU-4070-NV").Id,
                LocationId = locations.Single(l => l.Name == "Headquarters").Id,
                Amount = 4,
                CapacityCost = 4
            },
            new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = products.Single(p => p.Sku == "MON-27-UDEL").Id,
                LocationId = locations.Single(l => l.Name == "London Branch").Id,
                Amount = 10,
                CapacityCost = 7
            },
            new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = products.Single(p => p.Sku == "PER-MX3-LOGI").Id,
                LocationId = locations.Single(l => l.Name == "Ireland").Id,
                Amount = 153,
                CapacityCost = 1
            },
            new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = products.Single(p => p.Sku == "GPU-4070-NV").Id,
                LocationId = locations.Single(l => l.Name == "London Branch").Id,
                Amount = 6,
                CapacityCost = 4
            },
            new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = products.Single(p => p.Sku == "GPU-4080-NV").Id,
                LocationId = locations.Single(l => l.Name == "London Branch").Id,
                Amount = 3,
                CapacityCost = 4
            },
            new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = products.Single(p => p.Sku == "GPU-4090-NV").Id,
                LocationId = locations.Single(l => l.Name == "Headquarters").Id,
                Amount = 2,
                CapacityCost = 8
            },
            new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = products.Single(p => p.Sku == "GPU-7800-AMD").Id,
                LocationId = locations.Single(l => l.Name == "Ireland").Id,
                Amount = 5,
                CapacityCost = 3
            },
            new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = products.Single(p => p.Sku == "GPU-7900-AMD").Id,
                LocationId = locations.Single(l => l.Name == "London Branch").Id,
                Amount = 3,
                CapacityCost = 5
            },
            new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = products.Single(p => p.Sku == "CPU-I9-13900K").Id,
                LocationId = locations.Single(l => l.Name == "Headquarters").Id,
                Amount = 8,
                CapacityCost = 2
            },
            new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = products.Single(p => p.Sku == "CPU-I7-13700K").Id,
                LocationId = locations.Single(l => l.Name == "Ireland").Id,
                Amount = 10,
                CapacityCost = 2
            },
            new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = products.Single(p => p.Sku == "CPU-R9-7950X").Id,
                LocationId = locations.Single(l => l.Name == "London Branch").Id,
                Amount = 6,
                CapacityCost = 2
            },
            new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = products.Single(p => p.Sku == "CPU-R7-7800X3D").Id,
                LocationId = locations.Single(l => l.Name == "Headquarters").Id,
                Amount = 4,
                CapacityCost = 2
            },
            new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = products.Single(p => p.Sku == "MB-Z790E-ASUS").Id,
                LocationId = locations.Single(l => l.Name == "Headquarters").Id,
                Amount = 5,
                CapacityCost = 3
            },
            new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = products.Single(p => p.Sku == "MB-B650-MSI").Id,
                LocationId = locations.Single(l => l.Name == "London Branch").Id,
                Amount = 7,
                CapacityCost = 3
            },
            new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = products.Single(p => p.Sku == "MB-X670-GIGA").Id,
                LocationId = locations.Single(l => l.Name == "Ireland").Id,
                Amount = 4,
                CapacityCost = 3
            },
            new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = products.Single(p => p.Sku == "RAM-32G-DDR5-COR").Id,
                LocationId = locations.Single(l => l.Name == "Headquarters").Id,
                Amount = 20,
                CapacityCost = 1
            },
            new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = products.Single(p => p.Sku == "RAM-64G-Z5-GSK").Id,
                LocationId = locations.Single(l => l.Name == "London Branch").Id,
                Amount = 12,
                CapacityCost = 2
            },
            new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = products.Single(p => p.Sku == "SSD-2TB-990PRO").Id,
                LocationId = locations.Single(l => l.Name == "Ireland").Id,
                Amount = 15,
                CapacityCost = 2
            },
            new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = products.Single(p => p.Sku == "SSD-1TB-SN850X").Id,
                LocationId = locations.Single(l => l.Name == "Headquarters").Id,
                Amount = 18,
                CapacityCost = 1
            },
            new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = products.Single(p => p.Sku == "PER-HUNT-RAZER").Id,
                LocationId = locations.Single(l => l.Name == "London Branch").Id,
                Amount = 9,
                CapacityCost = 1
            }
        };

        db.Categories.AddRange(categories);
        db.Locations.AddRange(locations);
        db.Products.AddRange(products);
        db.Inventory.AddRange(inventory);
        db.SaveChanges();
    }
}
