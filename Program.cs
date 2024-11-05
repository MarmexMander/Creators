using Creators.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using Creators.Services;


var builder = WebApplication.CreateBuilder(args);
//var connectionString = builder.Configuration.GetConnectionString("CreatorsDbContextConnection") ?? throw new InvalidOperationException("Connection string 'CreatorsDbContextConnection' not found.");

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
});

builder.Services.AddDefaultIdentity<Creators.Models.CreatorUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<CreatorsDbContext>();
builder.Services.AddScoped<IMediaFileManager, LocalMediaFileManager>();

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
