using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.DTOs;
public class CartResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public List<CartItemResponse> Items { get; set; } = new();

    public decimal TotalAmount { get; set; }
}