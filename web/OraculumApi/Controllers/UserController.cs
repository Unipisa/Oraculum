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

    /// <summary>
    /// Retrieves the currently authenticated user's info
    /// </summary>
    /// <remarks></remarks>
    /// <response code="200">Returns the user's profile information, currently only roles.</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [Route("me")]
    [ValidateModelState]
    [DynamicAuthorize("frontoffice")]
    [SwaggerOperation("GetUserprofileInformations")]
    [SwaggerResponse(200, Type = typeof(UserProfile), Description = "User info")]
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

    /// <summary>
    /// Login by OIDC
    /// </summary>
    /// Login then redirect to url provided
    /// <remarks>
    /// </remarks>
    /// <response code="200">Logged in successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [Route("login-oidc")]
    public IActionResult LoginOIDC(String redirectUrl)
    {
        return Redirect(redirectUrl);   // TODO: revise me, check for security issues
    }

    /// <summary>
    /// Retrieves OpenID Connect (OIDC) configuration information for the current tenant
    /// </summary>
    /// <returns>An IActionResult containing OIDC configuration details such as authority and client ID.</returns>
    /// <response code="200">Returns OIDC configuration information.</response>
    /// <response code="500">If TenantId is not found in context or configuration, returns an internal server error.</response>
    [HttpGet]
    [Route("oidc-info")]
    [ProducesResponseType(typeof(OidcInfoResponse), StatusCodes.Status200OK)] // Assuming a response model exists
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult OidcInfo()
    {
        // Attempt to retrieve the tenantId from the HttpContext
        var tenantId = HttpContext.Items["TenantId"] as string;

        // Check if the tenantId is present
        if (tenantId == null)
            return StatusCode(StatusCodes.Status500InternalServerError, "TenantId not found in context");

        // Verify the tenantId exists within the application configuration
        if (!_configuration.GetSection("Tenants").GetChildren().Any(x => x.Key == tenantId))
            return StatusCode(StatusCodes.Status500InternalServerError, "TenantId not found in configuration");

        // Construct and return the OIDC information response
        return Ok(new
        {
            Authority = _configuration.GetSection("Tenants").GetSection(tenantId)["Security:OIDC:Authority"],
            ClientId = _configuration.GetSection("Tenants").GetSection(tenantId)["Security:OIDC:ClientId"],
        });
    }

    /// <summary>
    /// Response model for the OIDCInfo endpoint.
    /// </summary>
    public class OidcInfoResponse
    {
        /// <summary>
        /// The authority URL for OIDC.
        /// </summary>
        /// <example>https://example-oidc.com</example>
        public string Authority { get; set; }

        /// <summary>
        /// The client ID for OIDC.
        /// </summary>
        /// <example>oraculum</example>
        public string ClientId { get; set; }
    }
}
