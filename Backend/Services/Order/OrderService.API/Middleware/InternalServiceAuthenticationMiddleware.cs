namespace OrderService.API.Middleware;

public class InternalServiceAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public InternalServiceAuthenticationMiddleware(
        RequestDelegate next,
        IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var serviceKey =
            context.Request.Headers["X-Service-Key"].FirstOrDefault();

        var configuredKey =
            _configuration["InternalServices:ServiceKey"];

        if (!string.IsNullOrWhiteSpace(serviceKey) &&
            !string.IsNullOrWhiteSpace(configuredKey) &&
            serviceKey == configuredKey)
        {
            context.Items["IsInternalService"] = true;
        }

        await _next(context);
    }
}