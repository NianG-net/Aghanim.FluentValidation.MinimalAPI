using FluentValidation;

namespace Validation.Sample;

public record WeatherRequest1(string Summary);
public class WeatherRequestValidator2 : AbstractValidator<WeatherRequest1>
{
    readonly string[] summaries =
    [
        "Freezing", "Bracing", "Chilly"
    ];
    public WeatherRequestValidator2()
    {
        RuleFor(x => x.Summary).Must(x => summaries.Contains(x))
            .WithMessage(x => $"summary must exist summaries:[{string.Join(',', summaries)}]");
    }

}