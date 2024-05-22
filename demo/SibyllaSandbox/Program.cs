using Oraculum;
using Lib.AspNetCore.ServerSentEvents;
using SibyllaSandbox;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(builder =>
{
    builder.IdleTimeout = TimeSpan.FromMinutes(20);
    builder.Cookie.HttpOnly = true;
    builder.Cookie.IsEssential = true;
});

var _configuration = builder.Configuration;
var _env = builder.Environment;
builder.Services.AddSingleton<SibyllaManager>(new SibyllaManager(new Oraculum.OraculumConfiguration()
{
    DefaultProvider = new Oraculum.Providers.Facts.WeaviateOptions()
    {
        
        BaseUrl = _configuration["Weaviate:ServiceEndpoint"],
        ApiKey = _configuration["Weaviate:ApiKey"]
    },
    OpenAIModel = new OpenAIOptions()
    {
        ApiKey = _configuration["OpenAI:ApiKey"],
        Organization = _configuration["OpenAI:OrgId"]
    },
    AzureModel = new AzureOptions()
    {
        ApiKey = _configuration["Azure:ApiKey"],
        ResourceName = _configuration["Azure:ResourceName"],
        DeploymentId = _configuration["Azure:DeploymentId"],
        EndPoint = _configuration["EndPoint"]
    },
    ModelProvider = _configuration["GPTProvider"] == "Azure" ? ProviderType.Azure : ProviderType.OpenAi,
    LocalModel = new LocalModelOptions()
    {
        ApiKey = _configuration["Local:ApiKey"],
        EndPoint = _configuration["EndPoint"]
    }
}, Path.Combine(_env.ContentRootPath, "SibyllaeConf")));

builder.Services.AddServerSentEvents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapServerSentEvents("/MessagesStream");

app.Run();
