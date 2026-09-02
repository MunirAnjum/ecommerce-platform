using OrderService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Entities;
public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}