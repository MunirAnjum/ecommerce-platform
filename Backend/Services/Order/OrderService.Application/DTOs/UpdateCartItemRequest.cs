
namespace OrderService.Application.DTOs;

public class UpdateCartItemRequest
{
    public int WeightInGrams { get; set; } = 250;

    public int Quantity { get; set; }
}