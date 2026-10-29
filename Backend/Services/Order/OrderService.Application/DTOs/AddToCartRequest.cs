
namespace OrderService.Application.DTOs;

public class AddToCartRequest
{
    public Guid ProductId { get; set; }

    public int WeightInGrams { get; set; } = 250;

    public int Quantity { get; set; }
}
