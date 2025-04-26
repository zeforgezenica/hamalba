using hamalba.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using hamalba.Models;
using hamalba.DataBase;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.AspNetCore.Identity.UI.Services;
using hamalba.Helpers;
using hamalba.Filters;
using Serilog;
using Microsoft.Extensions.Options;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File(
        path: "Logs/global-log.txt",
        rollingInterval: RollingInterval.Infinite,
        shared: true
    )
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddJsonFile("emailsettings.local.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.Host.UseSerilog();

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<DatabaseConnection>();
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddRazorPages();
builder.Services.AddScoped<ActivityLogFilter>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.User.RequireUniqueEmail = true;
});

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(9, 0, 0))
    ));

builder.Services.AddIdentity<Korisnik, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddHostedService<OglasiStatusService>();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await RoleSeeder.SeedRolesAndAdmin(services);
}

app.Run();
