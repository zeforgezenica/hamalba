using hamalba.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using hamalba.Models;
using hamalba.DataBase;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Registruj servise
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<DatabaseService>();

// Entity Framework + MySQL (Pomelo)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(9, 0, 0)) // prilagodi ako tvoj MySQL nije verzija 9
    ));

builder.Services.AddDefaultIdentity<Korisnik>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

// ASP.NET Identity setup
builder.Services.AddIdentity<Korisnik, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

//HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

//  Autentifikacija i autorizacija
app.UseAuthentication(); // MORA biti prije Authorization
app.UseAuthorization();

// Rute
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

