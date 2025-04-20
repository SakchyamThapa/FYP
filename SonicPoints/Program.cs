using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SonicPoints.Data;
using SonicPoints.Models;
using SonicPoints.Repositories;
using SonicPoints.Services;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ------------------ DATABASE & IDENTITY ------------------

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ------------------ JWT CONFIG ------------------

var jwtSettings = builder.Configuration.GetSection("Jwt");
var utf8Key = Encoding.UTF8.GetBytes(jwtSettings["Key"]); // ✅ Use UTF8, not Base64

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(utf8Key), // ✅ Corrected key
        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role,
        ClockSkew = TimeSpan.FromMinutes(1)
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("❌ AUTHENTICATION FAILED: " + context.Exception.Message);
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            Console.WriteLine("📨 TOKEN RECEIVED: " + (context.Token ?? "NONE"));
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var userId = context.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = context.Principal.Identity?.Name;
            var roles = context.Principal.FindAll(ClaimTypes.Role).Select(r => r.Value);
            Console.WriteLine($"✅ AUTH SUCCESS: userId = {userId}, username = {username}");
            Console.WriteLine("Roles: " + string.Join(", ", roles));
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine("🚫 JWT CHALLENGE: " + context.AuthenticateFailure?.Message);
            return Task.CompletedTask;
        }
    };
});

// ------------------ COOKIE REDIRECT OVERRIDE ------------------

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

// ------------------ SWAGGER CONFIG ------------------

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SonicPoints API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter 'Bearer {your JWT token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ------------------ CORS POLICY ------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ------------------ DEPENDENCY INJECTION ------------------

builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IRewardRepository, RewardRepository>();
builder.Services.AddScoped<ILeaderboardRepository, LeaderboardRepository>();
builder.Services.AddScoped<IProjectAuthorizationService, ProjectAuthorizationService>();
builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Avoid duplicate call

var app = builder.Build();

// ------------------ ROLE SEEDING ------------------

async Task EnsureRolesCreatedAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "Admin", "Manager", "Checker", "Member" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            Console.WriteLine($"✅ Role '{role}' created.");
        }
    }
}

// ------------------ PIPELINE ------------------

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

await EnsureRolesCreatedAsync(app);

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication(); // 🔐 Required before UseAuthorization
app.UseAuthorization();

app.MapControllers();
app.Run();
