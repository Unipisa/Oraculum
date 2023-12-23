using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using OraculumApi.Attributes;
using OraculumApi.Models.BackOffice;
using Oraculum;
using System.Text;
using System.Runtime.CompilerServices;
using Microsoft.OpenApi.Any;
using Microsoft.AspNetCore.Http.HttpResults;
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
    [Authorize]
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
}
