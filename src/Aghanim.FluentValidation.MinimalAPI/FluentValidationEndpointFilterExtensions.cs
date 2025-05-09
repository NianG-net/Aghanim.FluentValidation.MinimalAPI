using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Http;

public static class FluentValidationAutoValidationExtensions
{
    public static IServiceCollection AddValidatorsFromAssemblyContaining<TValidator>(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        where TValidator : global::FluentValidation.IValidator
    {
        throw new NotSupportedException();
    }
}
