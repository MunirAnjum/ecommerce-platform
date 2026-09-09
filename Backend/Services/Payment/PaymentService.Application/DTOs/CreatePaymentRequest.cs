using PaymentService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.DTOs
{
    public class CreatePaymentRequest
    {
        public Guid OrderId { get; set; }

        public PaymentMethod Method { get; set; }
    }
}