var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
	options.AddPolicy("Frontend", policy =>
		policy.WithOrigins(
			"http://localhost:4200",
			"https://gilded-chaja-6d09f4.netlify.app",
			"https://parkease-apigateway.onrender.com")
			.AllowAnyHeader()
			.AllowAnyMethod());
});

builder.Services.AddReverseProxy()
	.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseCors("Frontend");
app.MapGet("/", () => "ParkEase API Gateway is running.");
app.MapReverseProxy();

app.Run();
