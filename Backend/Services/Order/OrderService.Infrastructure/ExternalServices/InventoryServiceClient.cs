using global::OrderService.Application.Interfaces;
using OrderService.Application.DTOs;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using OrderService.Application.Exceptions;

namespace OrderService.Infrastructure.ExternalServices;

public class InventoryServiceClient : IInventoryServiceClient
{
    private readonly HttpClient _httpClient;

    public InventoryServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task ReserveStockAsync(Guid productId, int quantity)
    {
        var request = new ReserveStockRequest
        {
            Quantity = quantity
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"api/inventory/{productId}/reserve",
            request);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var body = await response.Content.ReadAsStringAsync();

            var message = "Insufficient stock. Please update your cart.";

            try
            {
                using var document = JsonDocument.Parse(body);

                if (document.RootElement.TryGetProperty(
                        "message", out var messageElement))
                {
                    message = messageElement.GetString() ?? message;
                }
            }
            catch (JsonException)
            {
                // Keep the safe fallback message.
            }

            throw new InsufficientStockException(message);
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task ReleaseStockAsync(Guid productId, int quantity)
    {
        var request = new
        {
            Quantity = quantity
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"api/inventory/{productId}/release",
            request);

        response.EnsureSuccessStatusCode();
    }

    public async Task ConfirmStockAsync(Guid productId, int quantity)
    {
        var request = new
        {
            Quantity = quantity
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"api/inventory/{productId}/confirm",
            request);

        response.EnsureSuccessStatusCode();
    }
}
