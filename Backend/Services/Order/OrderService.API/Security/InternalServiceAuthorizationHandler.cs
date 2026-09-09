using Microsoft.AspNetCore.Authorization;

namespace OrderService.API.Security;

public class InternalServiceAuthorizationHandler
    : AuthorizationHandler<InternalServiceRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        InternalServiceRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated == true &&
            context.User.Identity.AuthenticationType == "InternalService")
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}