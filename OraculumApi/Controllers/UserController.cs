using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using OraculumApi.Attributes;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using OraculumApi.Models.User;

namespace OraculumApi.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1")]
public class UserController : Controller
{

    private readonly ILogger<BackOfficeController> _logger;
    private readonly IConfiguration _configuration;

    public UserController(ILogger<BackOfficeController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet]
    [Route("me")]
    [ValidateModelState]
    [DynamicAuthorize("frontoffice")]
    [SwaggerOperation("GetUserprofileInformations")]
    public Task<IActionResult> GetUserprofileInformations()
    {
        UserProfile userProfile = new UserProfile
        {
            Roles = new List<string>()
        };

        foreach (Claim claim in User.Claims)
        {
            if (claim.Type == ClaimTypes.Role)
                userProfile.Roles.Add(claim.Value);
        }

        return Task.FromResult<IActionResult>(Ok(userProfile));
    }

    [HttpGet]
    [Route("login-oidc")]
    public IActionResult LoginOIDC(String redirectUrl){
        return Redirect(redirectUrl);   // TODO: revise me, check for security issues
    }

    [HttpGet]
    [Route("oidc-info")]
    public IActionResult OidcInfo(){
        // get tenantId from context
        var tenantId = HttpContext.Items["TenantId"] as string;

        if(tenantId == null)
            return StatusCode(StatusCodes.Status500InternalServerError, "TenantId not found in context");

        if(!_configuration.GetSection("Tenants").GetChildren().Any(x => x.Key == tenantId))
            return StatusCode(StatusCodes.Status500InternalServerError, "TenantId not found in configuration");

        return Ok(new { 
            Authority = _configuration.GetSection("Tenants").GetSection(tenantId)["Security:OIDC:Authority"],
            ClientId = _configuration.GetSection("Tenants").GetSection(tenantId)["Security:OIDC:ClientId"],
        });
    }
}
