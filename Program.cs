using RailwayHelloApp.Infrastructure;
using RailwayHelloApp.Models;
using RailwayHelloApp.Services;

EnvFileLoader.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddSingleton(DatabaseConfig.CreateDataSource());
builder.Services.AddSingleton<TestRecordRepository>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/db-status", async (TestRecordRepository repository, CancellationToken cancellationToken) =>
{
    var result = await repository.TestConnectionAsync(cancellationToken);
    return result.Success ? Results.Ok(result) : Results.Problem(result.Message, statusCode: 500);
});

app.MapGet("/api/test-records", async (TestRecordRepository repository, CancellationToken cancellationToken) =>
{
    var records = await repository.GetAllAsync(cancellationToken);
    return Results.Ok(records);
});

app.MapGet("/api/test-records/{id:int}", async (int id, TestRecordRepository repository, CancellationToken cancellationToken) =>
{
    var record = await repository.GetByIdAsync(id, cancellationToken);
    return record is null ? Results.NotFound() : Results.Ok(record);
});

app.MapPost("/api/test-records", async (SaveTestRecordRequest request, TestRecordRepository repository, CancellationToken cancellationToken) =>
{
    var validationError = Validate(request);
    if (validationError is not null)
    {
        return Results.BadRequest(new { message = validationError });
    }

    var created = await repository.InsertAsync(request, cancellationToken);
    return Results.Created($"/api/test-records/{created.Id}", created);
});

app.MapPut("/api/test-records/{id:int}", async (int id, SaveTestRecordRequest request, TestRecordRepository repository, CancellationToken cancellationToken) =>
{
    var validationError = Validate(request);
    if (validationError is not null)
    {
        return Results.BadRequest(new { message = validationError });
    }

    var updated = await repository.UpdateAsync(id, request, cancellationToken);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
});

app.MapDelete("/api/test-records/{id:int}", async (int id, TestRecordRepository repository, CancellationToken cancellationToken) =>
{
    var deleted = await repository.DeleteAsync(id, cancellationToken);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.Run();

static string? Validate(SaveTestRecordRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return "Title zorunludur.";
    }

    if (string.IsNullOrWhiteSpace(request.Category))
    {
        return "Category zorunludur.";
    }

    return null;
}
