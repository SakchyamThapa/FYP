using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using MVC.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure Entity Framework Core to use SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Data")));

// Enable logging services
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Configure maximum attachment size limits for file uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104_857_600; // 100 MB
});

// Configure IIS server options for large requests
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 104_857_600; // 100 MB
});

//manage in sepearte file

builder.Services.AddTransient<IEmailLogsRepo, EmailLogRepo>();
builder.Services.AddTransient<IAdminRepo, AdminRepo>();
builder.Services.AddTransient<ILoginRepo, LoginRepo>();
builder.Services.AddTransient<IRedeemRepo, RedeemRepo>();
builder.Services.AddTransient<IRegisterRepo, RegisterRepo>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Use developer-friendly error pages in development mode
    app.UseDeveloperExceptionPage();
}
else
{
    // Use error handler and HSTS in production
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware to enforce HTTPS redirection
app.UseHttpsRedirection();

// Middleware to serve static files like CSS, JS, images, etc.
app.UseStaticFiles();

// Configure request routing
app.UseRouting();

// Middleware for user authentication and authorization (if applicable)
app.UseAuthorization();

// Map controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=SendMail}/{action=Index}/{id?}");

// Run the application
app.Run();
