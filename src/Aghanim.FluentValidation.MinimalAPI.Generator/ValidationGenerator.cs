using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Aghanim.FluentValidation.MinimalAPI.Generator;
[Generator(LanguageNames.CSharp)]
public class ValidationGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG_GEN
        System.Diagnostics.Debugger.Launch();
#endif
        var validatorInvocations = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) =>
                node is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: Constant.AddValidatorsMethodName } },
            transform: static (context, token) =>
                context.SemanticModel.GetOperation(context.Node, token) as IInvocationOperation
        ).Where(static operation => operation is not null)!.Collect<IInvocationOperation>();

     
        context.RegisterImplementationSourceOutput(validatorInvocations, (source, operations) =>
        {
            var requestTypes = new HashSet<string>();
            var interceptsBuilder = new StringBuilder();

            foreach (var operation in operations)
            {
#pragma warning disable RSEXPERIMENTAL002
                var location = operation.SemanticModel.GetInterceptableLocation((InvocationExpressionSyntax)operation.Syntax);
                if (location is null) continue;

                var interceptsLocation = location.GetInterceptsLocationAttributeSyntax();
#pragma warning restore RSEXPERIMENTAL002

                foreach (var typeArgument in operation.TargetMethod.TypeArguments)
                {
                    var validatorTypes = GeneratorHelpers.GetAllTypeSymbols(typeArgument.ContainingAssembly, Constant.FluentValidationInterface).ToImmutableArray();
                    var serviceBuilder = new StringBuilder();

                    foreach (var (requestType, validatorType) in validatorTypes)
                    {
                        serviceBuilder.AppendLine(GeneratorHelpers.GenerateServiceDescriptor(requestType, validatorType));
                        requestTypes.Add(requestType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                    }

                    interceptsBuilder.AppendLine(GeneratorHelpers.GenerateInterceptMethod(interceptsLocation, serviceBuilder, operations.IndexOf(operation)));
                }
            }

            source.AddSource(Constant.GeneratedFileNameIntercepts, GeneratorHelpers.GenerateIntercept(interceptsBuilder));

            var extensionsBuilder = new StringBuilder();
            foreach (var requestType in requestTypes)
            {
                extensionsBuilder.AppendLine(GeneratorHelpers.CreateValidatorMapping(requestType));
            }

            source.AddSource(Constant.GeneratedFileNameExtensions, GeneratorHelpers.GenerateValidatorContent(extensionsBuilder));
        });
    }


}
