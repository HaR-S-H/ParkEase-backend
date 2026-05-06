using System.Text;
using AuthService.Data;
using AuthService.Helpers.Implementations;
using AuthService.Helpers.Interfaces;
using AuthService.Mappings;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException("JWT Key is missing in configuration.")))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrWhiteSpace(context.Token)
                    && context.Request.Cookies.TryGetValue("accessToken", out var accessToken)
                    && !string.IsNullOrWhiteSpace(accessToken))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddAutoMapper(typeof(AuthMappingProfile).Assembly);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IStorageService, S3StorageService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddHttpClient<INotificationDispatcher, NotificationDispatcher>(client =>
{
    var baseUrl = builder.Configuration["ServiceUrls:NotificationService"]
        ?? throw new InvalidOperationException("Missing configuration key: ServiceUrls:NotificationService");

    client.BaseAddress = new Uri(baseUrl.TrimEnd('/'));
    client.Timeout = TimeSpan.FromSeconds(6);
});
builder.Services.AddScoped<IAuthService, AuthService.Services.AuthService>();

var app = builder.Build();
app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        context.Response.ContentType = "application/json";

        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();

        if (exceptionFeature != null)
        {
            var ex = exceptionFeature.Error;

            int statusCode = ex switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                InvalidOperationException => StatusCodes.Status409Conflict,
                AppException appEx => appEx.StatusCode,
                _ => StatusCodes.Status500InternalServerError
            };

            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(new
            {
                message = ex.Message,
                stack = app.Environment.IsDevelopment() ? ex.StackTrace : null
            });
        }
    });
});
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply pending EF Core migrations
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await db.Database.MigrateAsync();
    }
}
catch (Exception ex)
{
    app.Logger.LogError($"Failed to apply migrations: {ex.Message}");
}

app.Run();