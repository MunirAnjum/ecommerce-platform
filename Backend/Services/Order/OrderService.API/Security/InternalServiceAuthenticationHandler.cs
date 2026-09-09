using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace OrderService.API.Security;

public class InternalServiceAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;

    public InternalServiceAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var serviceKey =
            Request.Headers["X-Service-Key"].FirstOrDefault();

        var configuredKey =
            _configuration["InternalServices:ServiceKey"];

        if (string.IsNullOrWhiteSpace(serviceKey) ||
            string.IsNullOrWhiteSpace(configuredKey))
        {
            return Task.FromResult(
                AuthenticateResult.NoResult());
        }

        if (serviceKey != configuredKey)
        {
            return Task.FromResult(
                AuthenticateResult.Fail("Invalid service key."));
        }

        var claims = new[]
        {
            new Claim(
                ClaimTypes.Name,
                "InternalService")
        };

        var identity = new ClaimsIdentity(
            claims,
            "InternalService");

        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(
            principal,
            "InternalService");

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}