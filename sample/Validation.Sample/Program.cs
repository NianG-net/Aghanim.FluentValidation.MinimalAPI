using Scalar.AspNetCore;

using System.Text.Json.Serialization;

using Validation.lib;
using Validation.Sample;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddValidatorsFromAssemblyContaining<WeatherRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<WeatherRequestValidator2>();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference();

var api = app.MapGroup("api");
api.MapGet("demo", () => 1);
api.MapPost("/weatherforecast", (WeatherRequest request, ILogger<Program> logger) =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            request.Summary
        )).ToArray();
    return forecast;
}).WithName("GetWeatherForecast");

api.AddFluentValidationEndpointFilter();


app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
[JsonSerializable(typeof(WeatherForecast[]))]
[JsonSerializable(typeof(WeatherRequest))]
[JsonSerializable(typeof(HttpValidationProblemDetails))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
   
}
