using GymManager.Api.Data;
using GymManager.Api.Middlewares;
using GymManager.Api.Models;
using GymManager.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Parbad;
using Parbad.Builder;
using Parbad.Gateway.Mellat;
using Parbad.Gateway.Melli;
using Parbad.Gateway.ParbadVirtual;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// ----- DbContext -----
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(config.GetConnectionString("DefaultConnection")));

// ----- Parbad (Payment) -----
var paymentSettings = new PaymentSettings();
builder.Configuration.GetSection("PaymentSettings").Bind(paymentSettings);

builder.Services.AddParbad()
    .ConfigureGateways(gateways =>
    {
        gateways.AddMellat()
            .WithAccounts(accounts =>
            {
                accounts.AddInMemory(account =>
                {
                    account.TerminalId = paymentSettings.Mellat.TerminalId;
                    account.UserName = paymentSettings.Mellat.UserName;
                    account.UserPassword = paymentSettings.Mellat.UserPassword;
                });
            });
    })
    .ConfigureHttpContext(httpContextBuilder => httpContextBuilder.UseDefaultAspNetCore())
    .ConfigureStorage(storageBuilder => storageBuilder.UseMemoryCache());



// ----- Auth (JWT) -----
var jwtKey = config["Jwt:Secret"] ?? throw new Exception("JWT Secret required");
var key = Encoding.ASCII.GetBytes(jwtKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true
    };
});

// ----- Authorization policies -----
builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("SuperAdminOnly", p => p.RequireClaim("role", "SuperAdmin"));
    opts.AddPolicy("GymAdminOnly", p => p.RequireClaim("role", "GymAdmin"));
    opts.AddPolicy("TrainerOnly", p => p.RequireClaim("role", "Trainer"));
    opts.AddPolicy("AthleteOnly", p => p.RequireClaim("role", "Athlete"));
});

// ----- App services registration -----
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGymService, GymService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddSingleton<IEmailService, SmtpEmailService>();

// global middleware, caching, etc.
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<GlobalExceptionMiddleware>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// run migrations on startup (dev convenience)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseParbadVirtualGateway();
app.MapControllers();
app.Run();
