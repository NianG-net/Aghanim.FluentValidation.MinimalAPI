using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aghanim.FluentValidation.MinimalAPI.Generator;

internal static class GeneratorHelpers
{
    private static string FormatCode(string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root = syntaxTree.GetRoot();
        return root.NormalizeWhitespace().ToFullString();
    }
    public static IEnumerable<(ITypeSymbol requestType, INamedTypeSymbol validatorType)> GetAllTypeSymbols(IAssemblySymbol assemblySymbol, string interfaceName)
    {

        if (assemblySymbol == null)
        {
            yield break;
        }

        var stack = new Stack<INamespaceOrTypeSymbol>();
        stack.Push(assemblySymbol.GlobalNamespace);

        while (stack.Count > 0)
        {
            var symbol = stack.Pop();

            if (symbol is INamespaceSymbol namespaceSymbol)
            {
                foreach (var member in namespaceSymbol.GetMembers())
                {
                    stack.Push(member);
                }
            }
            else if (symbol is INamedTypeSymbol typeSymbol)
            {
                var validatorInterface = typeSymbol.AllInterfaces.FirstOrDefault(x => x.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == interfaceName);

                if (validatorInterface is not null)
                {
                    yield return (validatorInterface.TypeArguments[0], typeSymbol);
                }

                foreach (var member in typeSymbol.GetTypeMembers())
                {
                    stack.Push(member);
                }
            }
        }
    }
    public static string GenerateInterceptMethod(string interceptsLocationAttribute, StringBuilder code, int index) =>
        $$"""
            {{interceptsLocationAttribute}}
            public static IServiceCollection AddValidatorsFromAssemblyContaining{{index}}<TValidator>(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
                where TValidator : global::FluentValidation.IValidator
            {
                {{code}}
                return services;
            }
        """;
    public static string GenerateIntercept(StringBuilder code) => $$"""
        namespace System.Runtime.CompilerServices
        {
            [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
            file sealed class InterceptsLocationAttribute : System.Attribute
            {
                public InterceptsLocationAttribute(int version, string data)
                {
                }
            }
        }
        {{FormatCode(
    $$""""
        namespace Microsoft.AspNetCore.Http.Generated
        {
            file static class GeneratorFluentValidationEndpointFilterExtensions
            {
                {{code}}
            }
        }
    """")}}
        """;
    public static string CreateValidatorMapping(string requestType)
         => $$"""{ typeof({{requestType}}).TypeHandle.Value, static (sp, obj) => Validator<{{requestType}}>(sp, obj) },""";
    public static string GenerateServiceDescriptor(ITypeSymbol typeSymbol, INamedTypeSymbol namedTypeSymbol) =>
        $"services.Add(new ServiceDescriptor(typeof(global::FluentValidation.IValidator<{typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>), typeof({namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}), serviceLifetime));";
    public static string GenerateValidatorContent(StringBuilder code) => $$"""
        namespace Microsoft.AspNetCore.Http;
        
        public static partial class FluentValidationEndpointFilterExtensions
        {

            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            public static TBuild AddFluentValidationEndpointFilter<TBuild>(this TBuild builder)
                where TBuild : IEndpointConventionBuilder
            {
                return builder.AddFluentValidationEndpointFilter<TBuild, DefaultFluentValidationEndpointFilter>();
            }
            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            public static TBuild AddFluentValidationEndpointFilter<TBuild, [global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors)] TFilterType>(this TBuild builder)
                where TBuild : IEndpointConventionBuilder
                where TFilterType : IFluentValidationEndpointFilter
            {
                return builder.AddEndpointFilter<TBuild, TFilterType>();
            }

        }


        public interface IFluentValidationEndpointFilter : IEndpointFilter
        {
            ValueTask<object> FluentValidationInvokeAsync(IEnumerable<global::FluentValidation.Results.ValidationFailure> validationResults);
            
            ValueTask<object> IEndpointFilter.InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
            {
                var endpoint = context.HttpContext.GetEndpoint();
                var acceptsMetadata = endpoint?.Metadata.GetMetadata<global::Microsoft.AspNetCore.Http.Metadata.AcceptsMetadata>();
                if (acceptsMetadata?.RequestType is not null)
                {


                    var index = ValidatorHelp.ArgumentIndex.GetOrAdd(endpoint.DisplayName, _ => context.Arguments.Select(x => x.GetType().TypeHandle).ToList().IndexOf(acceptsMetadata.RequestType.TypeHandle));
                    
                    var argument = context.Arguments[index];

                    if (argument is not null)
                    {
                        var result = ValidatorHelp.Match(acceptsMetadata.RequestType.TypeHandle.Value, argument, context.HttpContext.RequestServices);
                        if (result.Any())
                        {
                            return FluentValidationInvokeAsync(result);
                        }
                    }
                }

                return next(context);
            }
        }

        public class DefaultFluentValidationEndpointFilter : IFluentValidationEndpointFilter
        {

            public ValueTask<object> FluentValidationInvokeAsync(IEnumerable<global::FluentValidation.Results.ValidationFailure> validationResults)
            {
                var errors = validationResults.Aggregate(new Dictionary<string, string[]>(), static (dict, error) =>
                {
                    dict[error.PropertyName] = dict.TryGetValue(error.PropertyName, out var values) ? [.. values, error.ErrorMessage] : [error.ErrorMessage];

                    return dict;
                });
                return ValueTask.FromResult<object>(TypedResults.ValidationProblem(errors));
            }
        }
        file static class ValidatorHelp
        
        {
            public static IEnumerable<global::FluentValidation.Results.ValidationFailure> Validator<T>(IServiceProvider serviceProvider, object t)
            {
                var services = serviceProvider.GetServices<global::FluentValidation.IValidator<T>>();
                foreach (var item in services)
                {
                    var result = item.Validate(global::System.Runtime.CompilerServices.Unsafe.As<object, T>(ref t));
                    if (!result.IsValid)
                    {
                        foreach (var error in result.Errors)
                        {
                            yield return error;
                        }

                    }
                }
            }
            public static global::System.Collections.Concurrent.ConcurrentDictionary<string, int> ArgumentIndex { get; private set; } = new();

            readonly static Dictionary<nint, Func<IServiceProvider, object, IEnumerable<global::FluentValidation.Results.ValidationFailure>>> TypeDict = new()
            {
               {{FormatCode(code.ToString())}}
            };

            public static IEnumerable<global::FluentValidation.Results.ValidationFailure> Match(nint typeHandle, object obj, IServiceProvider serviceProvider)
            => TypeDict.TryGetValue(typeHandle, out var func) ? func.Invoke(serviceProvider, obj) : [];

        }
        """;
}
