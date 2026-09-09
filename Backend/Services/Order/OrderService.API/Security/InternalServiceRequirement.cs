using Microsoft.AspNetCore.Authorization;

namespace OrderService.API.Security;

public class InternalServiceRequirement
    : IAuthorizationRequirement
{
}