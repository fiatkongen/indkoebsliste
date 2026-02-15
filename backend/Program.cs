using Microsoft.EntityFrameworkCore;
using Backend;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ShoppingDb>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=database.db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShoppingDb>();
    db.Database.EnsureCreated();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/health", () => "ok");

app.MapGet("/api/items", async (ShoppingDb db) =>
    await db.Items
        .OrderBy(i => i.IsChecked)
        .ThenByDescending(i => i.CreatedAt)
        .ToListAsync());

app.MapPost("/api/items", async (ShoppingDb db, ShoppingItem item) =>
{
    if (string.IsNullOrWhiteSpace(item.Name))
        return Results.BadRequest(new { error = "Name is required" });
    item.Id = 0;
    item.IsChecked = false;
    item.CreatedAt = DateTime.UtcNow;
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/api/items/{item.Id}", item);
});

app.MapPut("/api/items/{id}", async (int id, ShoppingDb db, ShoppingItem input) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();
    item.Name = input.Name;
    item.Quantity = input.Quantity;
    item.AddedBy = input.AddedBy;
    item.IsChecked = input.IsChecked;
    await db.SaveChangesAsync();
    return Results.Ok(item);
});

app.MapPut("/api/items/{id}/toggle", async (int id, ShoppingDb db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();
    item.IsChecked = !item.IsChecked;
    await db.SaveChangesAsync();
    return Results.Ok(item);
});

app.MapDelete("/api/items/{id}", async (int id, ShoppingDb db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();
    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/api/items/checked", async (ShoppingDb db) =>
{
    var checkedItems = await db.Items.Where(i => i.IsChecked).ToListAsync();
    db.Items.RemoveRange(checkedItems);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }
