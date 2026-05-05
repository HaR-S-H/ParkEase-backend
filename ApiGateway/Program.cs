var builder = WebApplication.CreateBuilder(args);
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
	?? ["http://localhost:8080", "http://localhost:4200"];

builder.Services.AddCors(options =>
{
	options.AddPolicy("Frontend", policy =>
		policy.WithOrigins(corsOrigins)
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials());
});

builder.Services.AddReverseProxy()
	.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseCors("Frontend");
app.MapGet("/", () => "ParkEase API Gateway is running.");
app.MapReverseProxy();

app.Run();
