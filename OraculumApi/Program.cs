using System.Text.Json.Serialization;
using Oraculum;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

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

app.UseAuthorization();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.MapControllers();

app.Run();
