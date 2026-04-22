var builder = WebApplication.CreateBuilder(args);

// Read the port from the environment for Railway deployment.
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

app.MapGet("/", () => "Hello World from .NET Core on Railway!");
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
