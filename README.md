## Get Started
FluentValidation.AspNetCore can be installed using the Nuget package manager or the `dotnet` CLI.

```
dotnet add package Aghanim.FluentValidation.MinimalAPI
```
The following examples will make use of a `WeatherRequest` object which is validated using a `WeatherRequestValidator`. These classes are defined as follows:

```csharp
public record WeatherRequest(string Summary);

public class WeatherRequestValidator : AbstractValidator<WeatherRequest>
{
    readonly string[] summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];
    public WeatherRequestValidator()
    {
        RuleFor(x => x.Summary).Must(x => summaries.Contains(x))
            .WithMessage(x => $"summary must exist summaries:[{string.Join(',', summaries)}]");
    }

}
```
Here we use the `AddValidatorsFromAssemblyContaining` method to automatically register all validators in the same assembly as `WeatherRequestValidator` with the service provider.

```csharp
builder.Services.AddValidatorsFromAssemblyContaining<WeatherRequestValidator>();
```
api add FluentValidationEndpointFilter 

```csharp
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
```
