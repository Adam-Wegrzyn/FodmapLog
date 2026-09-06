using Azure.Identity;
using Core.CQRS;
using Core.Interfaces;
using Core.Services;
using DataAccess;
using DataAccess.Interfaces;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var appInsightsKey = builder.Configuration["ApplicationInsights:InstrumentationKey"];
if (!string.IsNullOrWhiteSpace(appInsightsKey))
{
    builder.Services.AddApplicationInsightsTelemetry(appInsightsKey);
}

var tenantId = builder.Configuration["AzureAd:TenantId"];
var clientId = builder.Configuration["AzureAd:ClientId"];
var clientSecret = builder.Configuration["AzureAd:ClientSecret"];

var keyVaultName = builder.Configuration["KeyVaultName"];
if (!string.IsNullOrEmpty(keyVaultName)
    && !string.IsNullOrWhiteSpace(tenantId)
    && !string.IsNullOrWhiteSpace(clientId)
    && !string.IsNullOrWhiteSpace(clientSecret))
{
    var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
    var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
    builder.Configuration.AddAzureKeyVault(keyVaultUri, credential);
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
               corsBuilder =>
               {
                   corsBuilder.WithOrigins("http://localhost:4200")
                                       .AllowAnyHeader()
                                       .AllowAnyMethod();
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IFodmapLogRepository, FodmapLogRepository>();
builder.Services.AddScoped<IFodmapLogService, FodmapLogService>();
builder.Services.AddScoped<IAudioTranscriptionService, AudioTranscriptionService>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddHttpClient();

builder.Services.AddScoped<JwtTokenService>();

var useLocalSqlite = builder.Configuration.GetValue("UseLocalSqlite", false);
builder.Services.AddDbContext<FodmapLogDbContext>(options =>
{
    if (useLocalSqlite)
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("LocalSqlite")
            ?? "Data Source=fodmaplog-local.db");
    }
    else if (builder.Environment.IsDevelopment())
    {
        options.UseSqlServer(builder.Configuration["devConnectionAzure"]);
    }
    else
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("prodConnection"));
    }
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetSymptomTypesQueryHandler>());

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddApiEndpoints()
.AddEntityFrameworkStores<FodmapLogDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    options.SignIn.RequireConfirmedAccount = false;
});

var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        NameClaimType = JwtRegisteredClaimNames.Sub,
        RoleClaimType = ClaimTypes.Role
    };
});

var googleClientSecret = builder.Configuration["googleAuthSecret"];
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
if (!string.IsNullOrWhiteSpace(googleClientSecret) && !string.IsNullOrWhiteSpace(googleClientId))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
    });
}

// Require authentication by default; mark login/register/OAuth with [AllowAnonymous]
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

if (useLocalSqlite)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FodmapLogDbContext>();
    db.Database.EnsureCreated();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapGroup(string.Empty)
    .AllowAnonymous()
    .MapIdentityApi<IdentityUser>();

// SPA shell must remain reachable without a JWT
app.MapFallbackToFile("/index.html").AllowAnonymous();

app.Run();
