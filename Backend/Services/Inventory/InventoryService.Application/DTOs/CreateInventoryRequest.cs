using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.DTOs;
public class CreateInventoryRequest
{
    public Guid ProductId { get; set; }

    public int InitialQuantity { get; set; }
}
