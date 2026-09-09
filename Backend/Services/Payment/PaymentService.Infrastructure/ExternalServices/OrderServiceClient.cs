using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using System.Net.Http.Json;

namespace PaymentService.Infrastructure.ExternalServices;

public class OrderServiceClient : IOrderServiceClient
{
    private readonly HttpClient _httpClient;

    public OrderServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<OrderDetailsResponse?> GetOrderByIdAsync(
        Guid orderId)
    {
        var response = await _httpClient.GetAsync(
            $"api/order/internal/{orderId}");

        if (response.StatusCode ==
            System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<OrderDetailsResponse>();
    }

    public async Task ConfirmPaymentAsync(Guid orderId)
    {
        var response = await _httpClient.PostAsync(
            $"api/order/internal/{orderId}/payment-confirmed", null);

        response.EnsureSuccessStatusCode();
    }
}