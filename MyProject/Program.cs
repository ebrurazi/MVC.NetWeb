using Microsoft.EntityFrameworkCore;
using SolutionSharingApp.Data;
using SolutionSharingApp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using CloudinaryDotNet;
using SolutionSharingApp.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Cloudinary ayarlarını yapılandır
var cloudinarySettingsSection = builder.Configuration.GetSection("Cloudinary");
if (cloudinarySettingsSection == null)
{
    throw new Exception("Cloudinary configuration section is missing in appsettings.json");
}

var cloudinarySettings = cloudinarySettingsSection.Get<CloudinarySettings>();
if (cloudinarySettings == null)
{
    throw new Exception("Cloudinary settings are not configured properly in appsettings.json");
}

var cloudinary = new Cloudinary(new Account(
    cloudinarySettings.CloudName,
    cloudinarySettings.ApiKey,
    cloudinarySettings.ApiSecret
));
builder.Services.AddSingleton(cloudinary);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<SolutionService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<EmailService>(provider => new EmailService(
    "smtp.gmail.com", 465, "torunmetal343@gmail.com", "cxhtzcrbjpjsvelk"
));

// HttpClient ekleyin ve NLPService'yi yapılandırın
builder.Services.AddHttpClient();
builder.Services.AddSingleton<NLPService>(sp => 
    new NLPService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<IConfiguration>(), sp.GetRequiredService<ILogger<NLPService>>()));

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Authentication ve Authorization ekleyin
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// Logging'i ekleyin
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication(); // UseAuthentication middleware'ini ekleyin
app.UseAuthorization();  // UseAuthorization middleware'ini ekleyin

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "ai",
    pattern: "AI/{action=GetAIResponse}",
    defaults: new { controller = "AI", action = "GetAIResponse" });

app.Run();
