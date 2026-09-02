using global::OrderService.Application.Interfaces;
using OrderService.Application.DTOs;
using System.Net.Http.Json;

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
}
