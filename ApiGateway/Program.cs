var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy()
	.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapGet("/", () => "ParkEase API Gateway is running.");
app.MapReverseProxy();

app.Run();
