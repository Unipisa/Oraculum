using System.Text.Json.Serialization;
using Oraculum;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.Reflection.Metadata.Ecma335;

var builder = WebApplication.CreateBuilder(args);
var authorizationAssertionRetriver = new AuthorizationAssertionRetriever();

// Add services to the container.
builder.Services.AddControllers()
.AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
builder.Services.AddDistributedMemoryCache();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddHttpContextAccessor();
builder.Services.AddApiVersioning(x =>
{
    x.DefaultApiVersion = new ApiVersion(1, 0);
    x.AssumeDefaultVersionWhenUnspecified = true;
    x.ReportApiVersions = true;
    x.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader());
})
    .AddMvc()
    .AddApiExplorer(o =>
{
    o.GroupNameFormat = "'v'VVV";
    o.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSession(builder =>
{
    builder.IdleTimeout = TimeSpan.FromMinutes(20);
    builder.Cookie.HttpOnly = true;
    builder.Cookie.IsEssential = true;
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var _configuration = builder.Configuration;
var _env = builder.Environment;
builder.Services.AddSingleton<SibyllaManager>(new SibyllaManager(new Oraculum.Configuration()
{
    WeaviateEndpoint = _configuration["Weaviate:ServiceEndpoint"],
    WeaviateApiKey = _configuration["Weaviate:ApiKey"],
    Provider = _configuration["GPTProvider"] == "Azure" ? OpenAI.ProviderType.Azure : OpenAI.ProviderType.OpenAi,
    OpenAIApiKey = _configuration["OpenAI:ApiKey"],
    OpenAIOrgId = _configuration["OpenAI:OrgId"],
    AzureOpenAIApiKey = _configuration["Azure:ApiKey"],
    AzureResourceName = _configuration["Azure:ResourceName"],
    AzureDeploymentId = _configuration["Azure:DeploymentId"]
}, Path.Combine(_env.ContentRootPath, "SibyllaeConf")));

if (!_configuration.GetSection("OIDC").Exists())
{
    if (!_configuration.GetSection("AllowAnonymous").Get<bool>())
    {
        throw new Exception("AllowAnonymous is not set to true and no authentication method is configured");
    }
}

if (_configuration.GetSection("OIDC").Exists())
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        };
    })
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.Authority = _configuration["OIDC:Authority"];
        options.ClientId = _configuration["OIDC:ClientId"];
        options.ClientSecret = _configuration["OIDC:ClientSecret"];
        options.GetClaimsFromUserInfoEndpoint = true;
        options.ResponseType = "code";
        options.Scope.Add("openid");
        options.SaveTokens = true;
    });
}

try
{
    builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("SysAdmin", policy => authorizationAssertionRetriver.retrieveAssertion(policy, _configuration, "SysAdmin"));
            options.AddPolicy("BackOffice", policy => authorizationAssertionRetriver.retrieveAssertion(policy, _configuration, "BackOffice"));
            options.AddPolicy("FrontOffice", policy => authorizationAssertionRetriver.retrieveAssertion(policy, _configuration, "FrontOffice"));
        });
}
catch (Exception)
{
    Console.WriteLine("Error while configuring authorization policies: AuthorizationRolesMap is not configured correctly");
    throw;
}


var app = builder.Build();

app.UseCookiePolicy(new CookiePolicyOptions
{
    Secure = CookieSecurePolicy.Always
});

//var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // May be this code is not needed anymore
    //   app.UseSwaggerUI(options =>
    //{
    //    foreach (var description in provider.ApiVersionDescriptions)
    //    {
    //        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
    //    }
    //});
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseSession();

if (_configuration.GetSection("AllowAnonymous").Get<bool>())
    app.MapControllers().AllowAnonymous();
else
    app.MapControllers();

app.Run();
