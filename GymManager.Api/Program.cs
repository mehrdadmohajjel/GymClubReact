using GymManager.Api.Data;
using GymManager.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// --- Configuration (appsettings.json has JWT secret, connection string etc.)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// CORS
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("FrontendDev", b =>
    {
        b.WithOrigins(configuration["FrontendUrl"] ?? "http://localhost:3000")
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials();
    });
});

// JWT auth
var jwtSecret = configuration["Jwt:Secret"] ?? throw new Exception("JWT Secret missing");
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});
builder.Services.AddAuthorization(opts => {
    opts.AddPolicy("SuperAdminOnly", p => p.RequireClaim("role", "SuperAdmin"));
    opts.AddPolicy("GymAdminOnly", p => p.RequireClaim("role", "GymAdmin"));
    opts.AddPolicy("TrainerOnly", p => p.RequireClaim("role", "Trainer"));
    opts.AddPolicy("AthleteOnly", p => p.RequireClaim("role", "Athlete"));
});


// dependency injection for app services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGymService, GymService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Global exception handling
builder.Services.AddSingleton<GlobalExceptionMiddleware>();

// rate limit (very simple)
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<SimpleRateLimiter>();

var app = builder.Build();

// apply migrations in dev (optional)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// middlewares
app.UseCors("FrontendDev");
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
