using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using VehicleService.Data;
using VehicleService.Models;
using VehicleService.Repositories;
using VehicleService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddDbContext<VehicleDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IVehicleService, VehicleService.Services.VehicleService>();

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
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");
app.MapControllers();

// Apply pending EF Core migrations
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<VehicleDbContext>();
        await db.Database.MigrateAsync();
    }
}
catch (Exception ex)
{
    app.Logger.LogError($"Failed to apply migrations: {ex.Message}");
}

app.Run();
