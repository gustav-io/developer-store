using Ambev.DeveloperEvaluation.WebApi.Data;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ambev.DeveloperEvaluation.Functional.Infrastructure;

public static class ApiClientExtensions
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>Logs in with the seeded development admin and sets the bearer token.</summary>
    public static async Task AuthenticateAsync(this HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth", new
        {
            email = DevelopmentSeeder.AdminEmail,
            password = DevelopmentSeeder.AdminPassword
        });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        var token = body.GetProperty("data").GetProperty("token").GetString();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static object NewSaleRequest(int quantity = 5, string customerName = "Maria Silva") => new
    {
        customerId = Guid.NewGuid(),
        customerName,
        branchId = Guid.NewGuid(),
        branchName = "Centro",
        items = new[]
        {
            new { productId = Guid.NewGuid(), productName = "Brahma 350ml", unitPrice = 10.0m, quantity }
        }
    };
}
