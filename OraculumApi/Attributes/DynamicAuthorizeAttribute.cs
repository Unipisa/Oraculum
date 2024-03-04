using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class DynamicAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _policyPrefix;

    public DynamicAuthorizeAttribute(string policyPrefix)
    {
        _policyPrefix = policyPrefix;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var authorizationService = context.HttpContext.RequestServices.GetService(typeof(IAuthorizationService)) as IAuthorizationService;

        if (authorizationService == null)
        {
            context.Result = new StatusCodeResult((int)System.Net.HttpStatusCode.InternalServerError);
            return;
        }

        var tenantId = context.HttpContext.Items["TenantId"] as string;

        if (tenantId == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        var policyName = AuthorizationPolicyNameRetriever.GetName(_policyPrefix, tenantId);
        try
        {
            var result = authorizationService.AuthorizeAsync(context.HttpContext.User, policyName).GetAwaiter().GetResult();

            if (!result.Succeeded)
            {
                if (context.HttpContext.User.Identity?.IsAuthenticated == false)
                {
                    context.Result = new UnauthorizedResult();
                }
                else
                {
                    context.Result = new ForbidResult();
                }
                return;
            }
        }
        catch (Exception)
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}
