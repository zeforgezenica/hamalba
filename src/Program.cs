using hamalba.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using hamalba.Models;
using hamalba.DataBase;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.AspNetCore.Identity.UI.Services;
using hamalba.Helpers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<DatabaseConnection>();

// Registruj servise
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddRazorPages();

// Entity Framework + MySQL (Pomelo)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(9, 0, 0))
    ));

builder.Services.AddTransient<IEmailSender, FakeEmailSender>();


// ONLY THIS (supports roles too)
builder.Services.AddIdentity<Korisnik, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


builder.Services.AddHostedService<OglasiStatusService>();

var app = builder.Build();

// HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Autentifikacija i autorizacija
app.UseAuthentication(); // MORA biti prije Authorization
app.UseAuthorization();

// Rute
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.MapRazorPages(); // If you're using scaffolded Identity UI (Register/Login)

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await RoleSeeder.SeedRolesAndAdmin(services);
}

app.Run();

