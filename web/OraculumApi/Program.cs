using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
});

// Add services to the container.
builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpClient();

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

var _configuration = builder.Configuration;

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Oraculum API",
        Description = "API for Oraculum Frontoffice and Backoffice services",
        // TermsOfService = new Uri("https://example.com/terms"),
        // Contact = new OpenApiContact
        // {
        //     Name = "Example Contact",
        //     Url = new Uri("https://example.com/contact")
        // },
        // License = new OpenApiLicense
        // {
        //     Name = "Example License",
        //     Url = new Uri("https://example.com/license")
        // }
    });

    // using System.Reflection;
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

if (!_configuration.GetSection("AllowAnonymous").Get<bool>())
{
    builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
}


var _env = builder.Environment;
builder.Services.AddSingleton<SibyllaManagerProvider>();
builder.Services.AddScoped(provider =>
{
    var sibyllaManagerProvider = provider.GetRequiredService<SibyllaManagerProvider>();
    return sibyllaManagerProvider.GetSibyllaManager();
});

builder.Services.AddSingleton<WeaviateDBProvider>();
builder.Services.AddScoped(provider =>
    {
        var weaviateDBProvider = provider.GetRequiredService<WeaviateDBProvider>();
        return weaviateDBProvider.GetDatabase();
    });

builder.Services.AddScoped(typeof(WeaviateRepository<>));
builder.Services.AddScoped(typeof(BaseService<>));
builder.Services.AddScoped(typeof(BaseController<,>));
builder.Services.AddScoped(typeof(ChatDetailRepository));
builder.Services.AddScoped(typeof(ChatDetailService));
builder.Services.AddScoped(typeof(DataIngestionController));
builder.Services.AddScoped(typeof(BackupService));
builder.Services.AddScoped(typeof(EvaluateService));
builder.Services.AddScoped<IDataIngestionService, DataIngestionService>();

if (_configuration.GetSection("Tenants").GetChildren().Any(tenant => !tenant.GetSection("Security").Exists()))
{
    if (!_configuration.GetSection("AllowAnonymous").Get<bool>())
    {
        throw new Exception("AllowAnonymous is not set to true and no authentication method is configured");
    }
}

MultitenantAuthBuilder authBuilder = new(builder.Services, _configuration);

if (!_configuration.GetSection("AllowAnonymous").Get<bool>())
{
    authBuilder.AddMultitenantAuthentication();
    builder.Services.AddSingleton<IAuthenticationSchemeProvider, MultitenantAuthenticationSchemeProvider>();
}

try
{
    authBuilder.AddMultitenantAuthorization();
}
catch (Exception)
{
    Console.WriteLine("Error while configuring authorization policies: AuthorizationRolesMap is not configured correctly");
    throw;
}

builder.Services.AddControllers(options =>
{
    options.Filters.Add<HttpResponseExceptionFilter>();
});

var app = builder.Build();

app.UseForwardedHeaders();

app.UseMiddleware<TenantIdentificationMiddleware>();

app.UseCookiePolicy(new CookiePolicyOptions
{
    Secure = CookieSecurePolicy.Always
});

//var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    if (!_configuration.GetSection("AllowAnonymous").Get<bool>())
    {
        app.UseSwaggerUI(options =>
        {
            // SwaggerUi does not seem to support multiple OAuth2 configurations currently
            // options.OAuthClientId(_configuration["Swagger:OIDC:ClientId"]);
            options.OAuthScopes("openid", "profile", "offline_access");
            options.OAuthUsePkce();
        });
    }
    else
    {
        app.UseSwaggerUI();
    }
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
