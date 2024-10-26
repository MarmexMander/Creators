using Creators.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Creators.Models;

var builder = WebApplication.CreateBuilder(args);
//var connectionString = builder.Configuration.GetConnectionString("CreatorsDbContextConnection") ?? throw new InvalidOperationException("Connection string 'CreatorsDbContextConnection' not found.");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDbContext<CreatorsDbContext>(b =>
{
    string user = System.Environment.GetEnvironmentVariable("POSTGRES_USER");
    string pwd = System.Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
    string db = System.Environment.GetEnvironmentVariable("POSTGRES_DB");
    b.UseNpgsql($"Server=db;Port=5432;Database={db};User Id={user};Password={pwd};");
});

builder.Services.AddDefaultIdentity<CreatorUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<CreatorsDbContext>();

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
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
