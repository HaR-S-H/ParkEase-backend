using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using PaymentService.Data;
using PaymentService.Models;
using PaymentService.Repositories;
using PaymentService.Services;
using QuestPDF.Infrastructure;

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

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient("BookingServiceClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:BookingService"]
        ?? throw new InvalidOperationException("Missing configuration: ServiceUrls:BookingService"));
});

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<StripeGateway>();
builder.Services.AddScoped<RazorpayGateway>();
builder.Services.AddScoped<IPaymentService, PaymentService.Services.PaymentService>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    db.Database.EnsureCreated();
}

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
}

app.UseCors("Frontend");
app.MapControllers();

app.Run();
