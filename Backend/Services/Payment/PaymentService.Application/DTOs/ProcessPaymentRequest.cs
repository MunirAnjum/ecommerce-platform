using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.DTOs
{
    public class ProcessPaymentRequest
    {

        public bool ShouldSucceed { get; set; } = true;
    }
}
