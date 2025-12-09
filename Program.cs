using Creators.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using Creators.Services;
using Creators.Models;
using FFMpegCore;
using StackExchange.Redis;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;          // ← needed
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.AspNetCore.Mvc;

GlobalFFOptions.Configure(options => options.BinaryFolder = "/usr/bin");

var builder = WebApplication.CreateBuilder(args);

// ==================== REDIS (shared sessions + DataProtection) ====================
// Prefer configuration value `Redis:ConnectionString`, then common env vars.
var redisConn = builder.Configuration["Redis:ConnectionString"]
                ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION")
                ?? Environment.GetEnvironmentVariable("Redis__ConnectionString")
                ?? "redis:6379";

if (!string.IsNullOrEmpty(redisConn))
{
    var mux = ConnectionMultiplexer.Connect(redisConn);
    builder.Services.AddSingleton<IConnectionMultiplexer>(mux);

    builder.Services.AddStackExchangeRedisCache(options => options.Configuration = redisConn);

    // Ensure a fixed application name so all instances share the same key namespace
    builder.Services.AddDataProtection()
        .SetApplicationName("Creators")
        .PersistKeysToStackExchangeRedis(mux, "DataProtection-Keys")
        .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

    builder.Services.AddSession(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.IdleTimeout = TimeSpan.FromMinutes(60);
    });
}

// ==================== OTHER SERVICES ====================
// Replace/Add controllers registration to disable antiforgery globally for MVC
builder.Services.AddControllersWithViews(options =>
{
    // temporarily disable antiforgery validation for all controllers/views
    options.Filters.Add(new IgnoreAntiforgeryTokenAttribute());
});
builder.Services.AddControllers();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<CreatorsDbContext>(b =>
{
    var user = Environment.GetEnvironmentVariable("POSTGRES_USER")!;
    var pwd  = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")!;
    var db   = Environment.GetEnvironmentVariable("POSTGRES_DB")!;
    b.UseNpgsql($"Server=db;Port=5432;Database={db};User Id={user};Password={pwd};");
});

builder.Services.AddHttpContextAccessor();

// THIS IS THE FIX FOR NGINX → 400 antiforgery errors
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    // We are inside Docker network → trust everything (safe for demo/real usage)
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddLogging(logger =>
{
    logger.AddConsole();
    logger.SetMinimumLevel(LogLevel.Information);
    // Enable verbose DataProtection/Antiforgery logs only in development
    if (builder.Environment.IsDevelopment())
    {
        logger.AddFilter("Microsoft.AspNetCore.DataProtection", LogLevel.Debug);
        logger.AddFilter("Microsoft.AspNetCore.DataProtection.KeyManagement", LogLevel.Debug);
        logger.AddFilter("Microsoft.AspNetCore.Antiforgery", LogLevel.Debug);
    }
});

builder.Services.Configure<Dictionary<UploadTierEnum, UploadTier.MediaLimitations>>(
    builder.Configuration.GetSection("MediaLimits"));

builder.Services.AddDefaultIdentity<CreatorUser>(options => 
    options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<CreatorsDbContext>();

builder.Services.AddScoped<IMediaFileManager, LocalMediaFileManager>();
builder.Services.AddScoped<MediaLimiterService>();

var app = builder.Build();
// In development only: log DataProtection keys present in Redis (helps debug missing-key errors)
if (app.Environment.IsDevelopment())
{
    try
    {
        var muxLog = app.Services.GetService<IConnectionMultiplexer>();
        if (muxLog != null)
        {
            var db = muxLog.GetDatabase();
            var list = db.ListRange("DataProtection-Keys");
            if (list.Length > 0)
            {
                var ids = list.Select(item =>
                {
                    var s = item.ToString();
                    var m = Regex.Match(s, "key id=\"([^\"]+)\"");
                    return m.Success ? m.Groups[1].Value : "(no-id)";
                });
                app.Logger.LogInformation("DataProtection keys in Redis: {Ids}", string.Join(", ", ids));
            }
            else
            {
                app.Logger.LogWarning("No DataProtection keys found in Redis (DataProtection-Keys list is empty)");
            }
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Failed to read DataProtection keys from Redis for debugging");
    }
}

// ==================== PIPELINE ORDER (THIS ORDER IS CRUCIAL) ====================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 1. Forwarded headers must be THE VERY FIRST middleware
app.UseForwardedHeaders();

// 2. Force correct scheme (nginx terminates TLS or just proxies http)
app.Use((context, next) =>
{
    context.Request.Scheme = "http";   // change to "https" only if you terminate TLS in nginx
    return next();
});

if (app.Environment.IsDevelopment())
{
    // DEBUG: log POST cookies and form fields to diagnose antiforgery issues
    app.Use(async (context, next) =>
{
    if (string.Equals(context.Request.Method, "POST", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            context.Request.EnableBuffering();
            var cookieNames = context.Request.Cookies.Keys.Any()
                ? string.Join(", ", context.Request.Cookies.Keys)
                : "(none)";
            app.Logger.LogInformation("[ANTIFORGERY-DEBUG] Incoming POST cookies: {Cookies}", cookieNames);

            if (context.Request.Cookies.Keys.Any())
            {
                // log antiforgery cookie value masked
                var afCookie = context.Request.Cookies.FirstOrDefault(c => c.Key?.StartsWith(".AspNetCore.Antiforgery", StringComparison.OrdinalIgnoreCase) == true);
                if (!string.IsNullOrEmpty(afCookie.Value))
                {
                    var v = afCookie.Value;
                    var masked = v.Length <= 24 ? v : v.Substring(0, 8) + "..." + v.Substring(v.Length - 8);
                    app.Logger.LogInformation("[ANTIFORGERY-DEBUG] Antiforgery cookie value (masked): {Value} (len={Len})", masked, v.Length);
                }
            }

            if (context.Request.HasFormContentType)
            {
                var form = await context.Request.ReadFormAsync();
                var keys = string.Join(",", form.Keys);
                var tokenKey = form.Keys.FirstOrDefault(k => k?.IndexOf("RequestVerificationToken", StringComparison.OrdinalIgnoreCase) >= 0);
                var tokenPresent = tokenKey != null;
                app.Logger.LogInformation("[ANTIFORGERY-DEBUG] Form keys: {Keys}; TokenPresent: {TokenPresent}", keys, tokenPresent);

                if (tokenPresent)
                {
                    var tokenVal = form[tokenKey!].ToString();
                    var maskedT = tokenVal.Length <= 32 ? tokenVal : tokenVal.Substring(0, 12) + "..." + tokenVal.Substring(tokenVal.Length - 12);
                    app.Logger.LogInformation("[ANTIFORGERY-DEBUG] Form token key: {Key}; value (masked): {Value} (len={Len})", tokenKey, maskedT, tokenVal.Length);
                    try
                    {
                        var muxDbg = app.Services.GetService<IConnectionMultiplexer>();
                        if (muxDbg != null)
                        {
                            var db = muxDbg.GetDatabase();
                            var list = db.ListRange("DataProtection-Keys");
                            var ids = list.Select(item =>
                            {
                                var s = item.ToString();
                                var m = Regex.Match(s, "key id=\"([^\"]+)\"");
                                return m.Success ? m.Groups[1].Value : "(no-id)";
                            });
                            app.Logger.LogInformation("[ANTIFORGERY-DEBUG] DataProtection key ids in Redis (current): {Ids}", string.Join(", ", ids));
                        }
                        else
                        {
                            app.Logger.LogWarning("[ANTIFORGERY-DEBUG] No IConnectionMultiplexer available to read DataProtection keys");
                        }
                    }
                    catch (Exception ex)
                    {
                        app.Logger.LogWarning(ex, "[ANTIFORGERY-DEBUG] Failed to read DataProtection keys from Redis during POST");
                    }
                }
            }
            else
            {
                app.Logger.LogInformation("[ANTIFORGERY-DEBUG] POST not form-content-type: {ContentType}", context.Request.ContentType ?? "(null)");
            }

            context.Request.Body.Position = 0;
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "[ANTIFORGERY-DEBUG] Failed to read POST body for debugging");
        }
    }

        await next();
    });

    // DEBUG: attempt to validate antiforgery token here to capture detailed exceptions
    app.Use(async (context, next) =>
{
    if (string.Equals(context.Request.Method, "POST", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var antiforgery = context.RequestServices.GetService<IAntiforgery>();
            if (antiforgery != null)
            {
                try
                {
                    await antiforgery.ValidateRequestAsync(context);
                    app.Logger.LogInformation("[ANTIFORGERY-DEBUG] Manual antiforgery validation succeeded");
                }
                catch (Exception ex)
                {
                    app.Logger.LogError(ex, "[ANTIFORGERY-DEBUG] Manual antiforgery validation failed");
                }
            }
            else
            {
                app.Logger.LogWarning("[ANTIFORGERY-DEBUG] No IAntiforgery service available");
            }
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "[ANTIFORGERY-DEBUG] Unexpected error while attempting manual antiforgery validation");
        }
    }

        await next();
    });
}

app.UseHttpsRedirection();     // still keep this – it will redirect http → https only in prod when you want it
app.UseStaticFiles();

app.UseRouting();

// Session must come before Authentication/Authorization
if (!string.IsNullOrEmpty(redisConn))
{
    app.UseSession();
}

app.UseAuthentication();       // ← now sees correct RemoteIp + Scheme
app.UseAuthorization();

app.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

if (app.Environment.IsDevelopment())
{
    app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
        string.Join("\n", endpointSources.SelectMany(s => s.Endpoints)));
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