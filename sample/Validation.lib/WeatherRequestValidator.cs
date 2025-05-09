using FluentValidation;

namespace Validation.lib;

public record WeatherRequest(string Summary);

public record TestRequest(string Summary);

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
public class TestRequestValidator : AbstractValidator<TestRequest>
{ 
}