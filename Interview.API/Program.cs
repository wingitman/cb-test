using Infrastructure;
using Infrastructure.Helpers;
using Interview.API;
using Interview.API.Contracts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<InMemDbContext>(options => options.UseInMemoryDatabase("Default"));
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");

# region InMemoryDB Setup
using (IServiceScope scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<InMemDbContext>();
    InitialDataSet.Seed(context);
}
# endregion

app.MapGet("/products", async (
    InMemDbContext db,
    CancellationToken ct,
    string search = "",
    string category = "") =>
{
    search = search.Trim();
    category = category.Trim();
    await Logger.DebugAsync($"Product request: Search: {search}, Category: {category}");

    var products = await db.Products
    .Join(
        db.Categories,
        p => p.CategoryId,
        c => (Guid?)c.CategoryId,
        (p, c) => new
        {
            Product = p,
            Category = c
        })
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
}).WithName("Get Products");

app.MapGet("/locations", async (InMemDbContext db, CancellationToken ct) =>
{
    var locations = await db.Locations
        .AsNoTracking()
        .OrderBy(location => location.Name)
        .ToListAsync(ct);

    var inventory = await db.Inventory
        .AsNoTracking()
        .Select(item => new
        {
            item.LocationId,
            item.Amount,
            item.CapacityCost
        })
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
}).WithName("Get Locations");

app.MapPost("/products/{productId:guid}/inventory", async (
    Guid productId,
    InventoryRequest request,
    InMemDbContext db,
    CancellationToken ct) =>
{
    if (request.Amount <= 0 || request.CapacityCost <= 0)
        return Results.BadRequest(new { message = "Amount and capacity cost must be greater than zero." });

    if (!await db.Products.AnyAsync(product => product.Id == productId, ct))
        return Results.NotFound(new { message = "Product was not found." });

    var location = await db.Locations.FindAsync([request.LocationId], ct);
    if (location is null)
        return Results.NotFound(new { message = "Location was not found." });

    var existingInventory = await db.Inventory
        .AnyAsync(item => item.ProductId == productId && item.LocationId == request.LocationId, ct);

    if (existingInventory)
        return Results.Conflict(new { message = "This product already has inventory at that location. Edit the existing record instead." });

    var usedCapacity = await InventoryCapacity.GetUsedAsync(db, location.Id, null, ct);
    var requestedCapacity = (long)request.Amount * request.CapacityCost;

    if (usedCapacity + requestedCapacity > location.Capacity)
        return Results.Conflict(new { message = $"The inventory would exceed {location.Name}'s capacity by {usedCapacity + requestedCapacity - location.Capacity}." });

    var inventory = new Infrastructure.Models.Inventory
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
}).WithName("Add Product Inventory");

app.MapPut("/product/{productId:guid}/active", async (
    Guid productId,
    InMemDbContext db,
    CancellationToken ct) =>
{
    Logger.Debug($"Request to toggle {productId}'s active setting");
    var product = await db.Products.FirstOrDefaultAsync(x => x.Id == productId);
    if (product == null)
        return Results.BadRequest("No such product found");
    product.IsActive = !product.IsActive;
    await db.SaveChangesAsync(ct);

    return Results.Ok(product.IsActive);
}).WithName("Toggle Product Active");

app.MapPut("/inventory/{inventoryId:guid}", async (
    Guid inventoryId,
    InventoryRequest request,
    InMemDbContext db,
    CancellationToken ct) =>
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

    var usedCapacity = await InventoryCapacity.GetUsedAsync(db, location.Id, inventoryId, ct);
    var requestedCapacity = (long)request.Amount * request.CapacityCost;

    if (usedCapacity + requestedCapacity > location.Capacity)
        return Results.Conflict(new { message = $"The inventory would exceed {location.Name}'s capacity by {usedCapacity + requestedCapacity - location.Capacity}." });

    inventory.LocationId = request.LocationId;
    inventory.Amount = request.Amount;
    inventory.CapacityCost = request.CapacityCost;
    await db.SaveChangesAsync(ct);

    return Results.Ok(inventory);
}).WithName("Update Inventory");

app.MapDelete("/inventory/{inventoryId:guid}", async (
    Guid inventoryId,
    InMemDbContext db,
    CancellationToken ct) =>
{
    var inventory = await db.Inventory.FindAsync([inventoryId], ct);
    if (inventory is null)
        return Results.NotFound(new { message = "Inventory was not found." });

    db.Inventory.Remove(inventory);
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
}).WithName("Delete Inventory");

app.MapPost("/locations", async (
    LocationRequest request,
    InMemDbContext db,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) ||
        string.IsNullOrWhiteSpace(request.Region) ||
        string.IsNullOrWhiteSpace(request.Country) ||
        request.Capacity <= 0)
    {
        return Results.BadRequest(new { message = "Location details are required and capacity must be greater than zero." });
    }

    var location = new Infrastructure.Models.Location
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
}).WithName("Add Location");

app.MapPut("/locations/{locationId:guid}", async (
    Guid locationId,
    LocationRequest request,
    InMemDbContext db,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) ||
        string.IsNullOrWhiteSpace(request.Region) ||
        string.IsNullOrWhiteSpace(request.Country) ||
        request.Capacity <= 0)
    {
        return Results.BadRequest(new { message = "Location details are required and capacity must be greater than zero." });
    }

    var location = await db.Locations.FindAsync([locationId], ct);
    if (location is null)
        return Results.NotFound(new { message = "Location was not found." });

    var usedCapacity = await InventoryCapacity.GetUsedAsync(db, locationId, null, ct);
    if (request.Capacity < usedCapacity)
        return Results.Conflict(new { message = $"Capacity cannot be lower than the current usage of {usedCapacity}." });

    location.Name = request.Name.Trim();
    location.Region = request.Region.Trim();
    location.Country = request.Country.Trim();
    location.Capacity = request.Capacity;
    await db.SaveChangesAsync(ct);

    return Results.Ok(location);
}).WithName("Update Location");

app.MapDelete("/locations/{locationId:guid}", async (
    Guid locationId,
    InMemDbContext db,
    CancellationToken ct) =>
{
    var location = await db.Locations.FindAsync([locationId], ct);
    if (location is null)
        return Results.NotFound(new { message = "Location was not found." });

    if (await db.Inventory.AnyAsync(item => item.LocationId == locationId, ct))
        return Results.Conflict(new { message = "Move all product inventory out of this location before removing it." });

    db.Locations.Remove(location);
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
}).WithName("Delete Location");


app.Run();
