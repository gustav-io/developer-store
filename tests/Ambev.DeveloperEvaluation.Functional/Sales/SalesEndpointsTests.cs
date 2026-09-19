using Ambev.DeveloperEvaluation.Functional.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Sales;

/// <summary>
/// End-to-end tests for /api/sales through the HTTP pipeline (auth, validation, error contract).
/// </summary>
public class SalesEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public SalesEndpointsTests(ApiFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> AuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        await client.AuthenticateAsync();
        return client;
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>(ApiClientExtensions.Json);

    [Fact(DisplayName = "Sales endpoints should require authentication")]
    public async Task Get_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/sales");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Create then get should return the sale with tiered discount applied")]
    public async Task CreateAndGet_AppliesDiscount()
    {
        var client = await AuthenticatedClientAsync();

        var create = await client.PostAsJsonAsync("/api/sales", ApiClientExtensions.NewSaleRequest(quantity: 10));
        create.StatusCode.Should().Be(HttpStatusCode.Created);

        var data = (await ReadJsonAsync(create)).GetProperty("data");
        var id = data.GetProperty("id").GetGuid();
        data.GetProperty("saleNumber").GetInt64().Should().BeGreaterThanOrEqualTo(1000);
        data.GetProperty("totalAmount").GetDecimal().Should().Be(80m);
        data.GetProperty("items")[0].GetProperty("discountPercent").GetDecimal().Should().Be(0.2m);

        var get = await client.GetAsync($"/api/sales/{id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadJsonAsync(get)).GetProperty("data").GetProperty("status").GetString().Should().Be("Active");
    }

    [Fact(DisplayName = "Creating with more than 20 units should return 400 BusinessRuleViolation")]
    public async Task Create_QuantityAbove20_Returns400()
    {
        var client = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/sales", ApiClientExtensions.NewSaleRequest(quantity: 21));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await ReadJsonAsync(response);
        error.GetProperty("type").GetString().Should().Be("BusinessRuleViolation");
        error.GetProperty("detail").GetString().Should().Contain("20");
    }

    [Fact(DisplayName = "Creating without items should return 400 ValidationError")]
    public async Task Create_NoItems_Returns400()
    {
        var client = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/sales", new
        {
            customerId = Guid.NewGuid(),
            customerName = "X",
            branchId = Guid.NewGuid(),
            branchName = "Y",
            items = Array.Empty<object>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ReadJsonAsync(response)).GetProperty("type").GetString().Should().Be("ValidationError");
    }

    [Fact(DisplayName = "Unknown sale should return 404 ResourceNotFound")]
    public async Task Get_Unknown_Returns404()
    {
        var client = await AuthenticatedClientAsync();

        var response = await client.GetAsync($"/api/sales/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await ReadJsonAsync(response)).GetProperty("type").GetString().Should().Be("ResourceNotFound");
    }

    [Fact(DisplayName = "Cancelling a sale twice should succeed then be rejected")]
    public async Task Cancel_Twice_SecondIsRejected()
    {
        var client = await AuthenticatedClientAsync();
        var create = await client.PostAsJsonAsync("/api/sales", ApiClientExtensions.NewSaleRequest());
        var id = (await ReadJsonAsync(create)).GetProperty("data").GetProperty("id").GetGuid();

        var first = await client.PatchAsync($"/api/sales/{id}/cancel", null);
        var second = await client.PatchAsync($"/api/sales/{id}/cancel", null);

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadJsonAsync(first)).GetProperty("data").GetProperty("status").GetString().Should().Be("Cancelled");
        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Cancelling an item should recalculate the total")]
    public async Task CancelItem_RecalculatesTotal()
    {
        var client = await AuthenticatedClientAsync();
        var create = await client.PostAsJsonAsync("/api/sales", ApiClientExtensions.NewSaleRequest(quantity: 2));
        var data = (await ReadJsonAsync(create)).GetProperty("data");
        var id = data.GetProperty("id").GetGuid();
        var itemId = data.GetProperty("items")[0].GetProperty("id").GetGuid();

        var response = await client.PatchAsync($"/api/sales/{id}/items/{itemId}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = (await ReadJsonAsync(response)).GetProperty("data");
        updated.GetProperty("totalAmount").GetDecimal().Should().Be(0m);
        updated.GetProperty("items")[0].GetProperty("isCancelled").GetBoolean().Should().BeTrue();
    }

    [Fact(DisplayName = "Update should replace items and header")]
    public async Task Update_ReplacesItems()
    {
        var client = await AuthenticatedClientAsync();
        var create = await client.PostAsJsonAsync("/api/sales", ApiClientExtensions.NewSaleRequest(quantity: 2));
        var id = (await ReadJsonAsync(create)).GetProperty("data").GetProperty("id").GetGuid();

        var response = await client.PutAsJsonAsync($"/api/sales/{id}", new
        {
            saleDate = DateTime.UtcNow,
            customerId = Guid.NewGuid(),
            customerName = "Renamed",
            branchId = Guid.NewGuid(),
            branchName = "Other branch",
            items = new[]
            {
                new { productId = Guid.NewGuid(), productName = "A", unitPrice = 10.0m, quantity = 4 },
                new { productId = Guid.NewGuid(), productName = "B", unitPrice = 5.0m, quantity = 1 }
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = (await ReadJsonAsync(response)).GetProperty("data");
        data.GetProperty("customerName").GetString().Should().Be("Renamed");
        data.GetProperty("items").GetArrayLength().Should().Be(2);
        data.GetProperty("totalAmount").GetDecimal().Should().Be(36m + 5m);
    }

    [Fact(DisplayName = "List should page, order and filter")]
    public async Task List_PagesOrdersAndFilters()
    {
        var client = await AuthenticatedClientAsync();
        var marker = $"List{Guid.NewGuid():N}";
        foreach (var quantity in new[] { 1, 5, 10 })
            (await client.PostAsJsonAsync("/api/sales", ApiClientExtensions.NewSaleRequest(quantity, $"{marker} Customer"))).EnsureSuccessStatusCode();

        var response = await client.GetAsync($"/api/sales?_page=1&_size=2&_order=totalAmount desc&customerName={marker}*");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync(response);
        body.GetProperty("totalCount").GetInt32().Should().Be(3);
        body.GetProperty("totalPages").GetInt32().Should().Be(2);
        var items = body.GetProperty("data").EnumerateArray().ToList();
        items.Should().HaveCount(2);
        items[0].GetProperty("totalAmount").GetDecimal().Should().Be(80m);
        items[1].GetProperty("totalAmount").GetDecimal().Should().Be(45m);
    }

    [Fact(DisplayName = "List with unknown order field should return 400 ValidationError")]
    public async Task List_UnknownOrderField_Returns400()
    {
        var client = await AuthenticatedClientAsync();

        var response = await client.GetAsync("/api/sales?_order=password desc");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ReadJsonAsync(response)).GetProperty("type").GetString().Should().Be("ValidationError");
    }

    [Fact(DisplayName = "Delete should remove the sale")]
    public async Task Delete_RemovesSale()
    {
        var client = await AuthenticatedClientAsync();
        var create = await client.PostAsJsonAsync("/api/sales", ApiClientExtensions.NewSaleRequest());
        var id = (await ReadJsonAsync(create)).GetProperty("data").GetProperty("id").GetGuid();

        (await client.DeleteAsync($"/api/sales/{id}")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetAsync($"/api/sales/{id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
