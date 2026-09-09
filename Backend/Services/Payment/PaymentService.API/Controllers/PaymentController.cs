using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PaymentService.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePaymentRequest request)
        {
            var userId = GetUserId();

            var payment = await _paymentService.CreateAsync(userId, request);

            return Ok(payment);
        }

        [HttpPost("{paymentId:guid}/process")]
        public async Task<IActionResult> Process(Guid paymentId, ProcessPaymentRequest request)
        {
            var userId = GetUserId();

            var payment = await _paymentService.ProcessAsync(userId, paymentId, request);

            return Ok(payment);
        }

        [HttpGet("{paymentId:guid}")]
        public async Task<IActionResult> GetById(Guid paymentId)
        {
            var userId = GetUserId();

            var payment = await _paymentService.GetByIdAsync(userId, paymentId);

            return Ok(payment);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyPayments()
        {
            var userId = GetUserId();

            var payment = await _paymentService.GetMyPaymentsAsync(userId);

            return Ok(payment);
        }

        [HttpPost("{paymentId:guid}/confirm-cod")]
        public async Task<IActionResult> ConfirmCashOnDelivery(Guid paymentId)
        {
            var userId = GetUserId();

            var payment = await _paymentService.ConfirmCashOnDeliveryAsync(userId, paymentId);

            return Ok(payment);
        }

        [HttpPost]
        private Guid GetUserId()
        {
            var value = 
                User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(!Guid.TryParse(value, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user identity.");
            }

            return userId;
        }
    }
}
