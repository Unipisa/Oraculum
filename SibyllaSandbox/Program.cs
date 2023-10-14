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
builder.Services.AddSingleton<SibyllaManager>(new SibyllaManager(new Oraculum.Configuration()
{
    WeaviateEndpoint = _configuration["Weaviate:ServiceEndpoint"],
    WeaviateApiKey = _configuration["Weaviate:ApiKey"],
    OpenAIApiKey = _configuration["OpenAI:ApiKey"],
    OpenAIOrgId = _configuration["OpenAI:OrgId"]
}, Path.Combine(_env.ContentRootPath, "SibyllaeConf")));

builder.Services.AddServerSentEvents();

var app = builder.Build();

var sses = app.Services.GetService<ServerSentEventsService>();
if (sses != null)
{
    sses.ClientConnected += (sender, e) =>
     {
         var session = e.Request.HttpContext.Session;
         if (session.IsAvailable && !session.Keys.Contains("SSEId"))
         {
             session.SetString("SSEId", e.Client.Id.ToString());
             session.CommitAsync().Wait();
         }
     };
}

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
