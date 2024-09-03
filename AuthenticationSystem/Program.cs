using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add cookie-based authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/CheckLogin";  // Redirect to login page if unauthorized
        options.AccessDeniedPath = "/Home/AccessDenied";  // Redirect to access denied page if needed
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);  // Set cookie expiration time
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Enable authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();
