using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Backend;

namespace backend.Tests;

public class ApiTests
{
    private static HttpClient CreateClient()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ShoppingDb>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<ShoppingDb>(opt => opt.UseSqlite(connection));
            });
        });

        return factory.CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("ok", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetItems_Empty()
    {
        var client = CreateClient();
        var items = await client.GetFromJsonAsync<List<ShoppingItem>>("/api/items");
        Assert.NotNull(items);
        Assert.Empty(items);
    }

    [Fact]
    public async Task PostItem_CreatesItem()
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/items", new { name = "Mælk", addedBy = "Rasmus", quantity = "1L" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var item = await response.Content.ReadFromJsonAsync<ShoppingItem>();
        Assert.NotNull(item);
        Assert.Equal("Mælk", item!.Name);
        Assert.Equal("Rasmus", item.AddedBy);
        Assert.Equal("1L", item.Quantity);
        Assert.False(item.IsChecked);
    }

    [Fact]
    public async Task ToggleItem()
    {
        var client = CreateClient();
        var post = await client.PostAsJsonAsync("/api/items", new { name = "Brød", addedBy = "Claire" });
        var created = await post.Content.ReadFromJsonAsync<ShoppingItem>();

        var toggle = await client.PutAsync($"/api/items/{created!.Id}/toggle", null);
        Assert.Equal(HttpStatusCode.OK, toggle.StatusCode);
        var toggled = await toggle.Content.ReadFromJsonAsync<ShoppingItem>();
        Assert.True(toggled!.IsChecked);

        var toggle2 = await client.PutAsync($"/api/items/{created.Id}/toggle", null);
        var toggled2 = await toggle2.Content.ReadFromJsonAsync<ShoppingItem>();
        Assert.False(toggled2!.IsChecked);
    }

    [Fact]
    public async Task UpdateItem()
    {
        var client = CreateClient();
        var post = await client.PostAsJsonAsync("/api/items", new { name = "Ost", addedBy = "Katja" });
        var created = await post.Content.ReadFromJsonAsync<ShoppingItem>();

        var update = await client.PutAsJsonAsync($"/api/items/{created!.Id}",
            new { name = "Gouda", addedBy = "Katja", quantity = "200g", isChecked = false });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var updated = await update.Content.ReadFromJsonAsync<ShoppingItem>();
        Assert.Equal("Gouda", updated!.Name);
    }

    [Fact]
    public async Task DeleteItem()
    {
        var client = CreateClient();
        var post = await client.PostAsJsonAsync("/api/items", new { name = "Smør", addedBy = "Kathrine" });
        var created = await post.Content.ReadFromJsonAsync<ShoppingItem>();

        var del = await client.DeleteAsync($"/api/items/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);
    }

    [Fact]
    public async Task DeleteChecked()
    {
        var client = CreateClient();
        await client.PostAsJsonAsync("/api/items", new { name = "A", addedBy = "R" });
        var post2 = await client.PostAsJsonAsync("/api/items", new { name = "B", addedBy = "R" });
        var item2 = await post2.Content.ReadFromJsonAsync<ShoppingItem>();
        await client.PutAsync($"/api/items/{item2!.Id}/toggle", null);

        var del = await client.DeleteAsync("/api/items/checked");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

        var remaining = await client.GetFromJsonAsync<List<ShoppingItem>>("/api/items");
        Assert.Single(remaining!);
        Assert.Equal("A", remaining[0].Name);
    }

    [Fact]
    public async Task Toggle_NotFound()
    {
        var client = CreateClient();
        var response = await client.PutAsync("/api/items/99999/toggle", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NotFound()
    {
        var client = CreateClient();
        var response = await client.DeleteAsync("/api/items/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
