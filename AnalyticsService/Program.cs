using AnalyticsService.Data;
using AnalyticsService.Models;
using AnalyticsService.Repositories;
using AnalyticsService.Services;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200", "http://127.0.0.1:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient("BookingServiceClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:BookingService"]
        ?? throw new InvalidOperationException("Missing configuration: ServiceUrls:BookingService"));
});

builder.Services.AddHttpClient("PaymentServiceClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:PaymentService"]
        ?? throw new InvalidOperationException("Missing configuration: ServiceUrls:PaymentService"));
});

builder.Services.AddHttpClient("ParkingLotServiceClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ParkingLotService"]
        ?? throw new InvalidOperationException("Missing configuration: ServiceUrls:ParkingLotService"));
});

builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService.Services.AnalyticsService>();

builder.Services.AddHangfire(config =>
{
    config.UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseMemoryStorage();
});
builder.Services.AddHangfireServer();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

            var statusCode = ex switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
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
    app.UseHangfireDashboard("/hangfire");
}

var occupancyCron = builder.Configuration["Hangfire:OccupancyCron"] ?? "*/5 * * * *";
using (var scope = app.Services.CreateScope())
{
    var recurringJobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobs.AddOrUpdate<IAnalyticsService>(
        "analytics.occupancy.logging",
        service => service.LogOccupancy(CancellationToken.None),
        occupancyCron);
}

app.UseCors("Frontend");
app.MapControllers();

app.Run();
