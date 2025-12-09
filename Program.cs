using Creators.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using Creators.Services;
using Creators.Models;
using FFMpegCore;
using StackExchange.Redis;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using System;



GlobalFFOptions.Configure(options => options.BinaryFolder = "/usr/bin");

var builder = WebApplication.CreateBuilder(args);
//var connectionString = builder.Configuration.GetConnectionString("CreatorsDbContextConnection") ?? throw new InvalidOperationException("Connection string 'CreatorsDbContextConnection' not found.");

// read redis connection (hardcoded default; do NOT read builder.Configuration)
var redisConn = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "redis:6379";

if (!string.IsNullOrEmpty(redisConn))
{
    // Distributed cache (Redis)
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConn;
    });

    // Keep and reuse a ConnectionMultiplexer
    var mux = ConnectionMultiplexer.Connect(redisConn);
    builder.Services.AddSingleton<IConnectionMultiplexer>(mux);

    // Persist DataProtection keys to Redis so all instances can unprotect cookies/tokens
    builder.Services.AddDataProtection()
        .PersistKeysToStackExchangeRedis(() => mux.GetDatabase(), "DataProtection-Keys");

    // Use session backed by IDistributedCache (Redis)
    builder.Services.AddSession(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.IdleTimeout = TimeSpan.FromMinutes(20);
    });
}

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddDbContext<CreatorsDbContext>(b =>
{
    string user = System.Environment.GetEnvironmentVariable("POSTGRES_USER");
    string pwd = System.Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
    string db = System.Environment.GetEnvironmentVariable("POSTGRES_DB");
    b.UseNpgsql($"Server=db;Port=5432;Database={db};User Id={user};Password={pwd};");
});


builder.Services.AddLogging( logger => {
    logger.AddConsole();
    logger.SetMinimumLevel(LogLevel.Debug );
});

builder.Services.Configure<Dictionary<UploadTierEnum, UploadTier.MediaLimitations>>(
    builder.Configuration.GetSection("MediaLimits"));
builder.Services.AddDefaultIdentity<CreatorUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<CreatorsDbContext>();
builder.Services.AddScoped<IMediaFileManager, LocalMediaFileManager>();
builder.Services.AddScoped<MediaLimiterService>();

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

if (!string.IsNullOrEmpty(redisConn))
{
    app.UseSession();
}

app.UseAuthorization();


app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); 
if (app.Environment.IsDevelopment())
{
    app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
        string.Join("\n", endpointSources.SelectMany(source => source.Endpoints)));
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
           Path.Combine(builder.Environment.ContentRootPath, "Resources")),
    RequestPath = "/resources"
});
app.MapControllers();
app.MapRazorPages();
app.Run();
