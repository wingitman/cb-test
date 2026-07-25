using Infrastructure;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<InMemDbContext>(options => options.UseInMemoryDatabase("Default"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

# region InMemoryDB Setup
using (IServiceScope scope = app.Services.CreateScope())
{
    bool change = false;
    var context = scope.ServiceProvider.GetRequiredService<InMemDbContext>();

    if (!context.Products.Any())
    {
        change = true;
        context.Products.AddRange(InitialDataSet.Get());
    }
    if (!context.Categories.Any())
    {
        change = true;
        context.Categories.AddRange(InitialDataSet.GetCategories());
    }

    if (change)
    {
        Logger.Debug("Writing InitialDataSet");
        context.SaveChanges();
    }
}
# endregion

app.MapGet("/", () => (object?)null).WithName("Default Endpoint");

app.MapGet("/products", async (InMemDbContext db, CancellationToken ct, string category = "") =>
{
    Logger.Debug($"Product request: Category: {category}");
    var categories = await db.Categories.Where(x =>
        x.IsActive &&
        (String.IsNullOrEmpty(category) || x.Name.ToString().Contains(category))
    ).ToListAsync();
    var products = await db.Products.Where(x =>
        x.IsActive
    ).ToListAsync();
    products = products.Where(x => categories.Any(c => c.CategoryId == x.CategoryId.GetValueOrDefault())).ToList();

    if (!products.Any())
        return Results.Ok("No available products");

    return Results.Ok(products);
}).WithName("Get Products");


app.Run();
