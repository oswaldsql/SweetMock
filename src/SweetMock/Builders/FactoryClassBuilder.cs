namespace SweetMock.Builders;

using System.Linq;
using Generation;
using Microsoft.CodeAnalysis;
using Utils;

/// <summary>
///     Provides methods to build mock classes.
/// </summary>
public static class FactoryClassBuilder
{
    /// <summary>
    ///     Builds the mock classes based on the provided type symbols.
    /// </summary>
    /// <returns>A string containing the generated mock classes.</returns>
    public static string Build(MockDetails details)
    {
        var builder = new CodeBuilder();

        builder.AddFileHeader().Add("#nullable enable");
        builder.Scope("namespace SweetMock", b =>
        {
            b.Documentation(doc => doc
                .Summary("Factory for creating mock objects."));

            b.Scope("internal static partial class Mock", c =>
            {
                var constructors = details.Target.Constructors.Where(Include).ToArray();
                foreach (var constructor in constructors)
                {
                    c.BuildFactoryMethod(details, constructor);
                }

                if (constructors.Length == 0)
                {
                    c.BuildFactoryMethod(details);
                }
            });
        });

        return builder.ToString();
    }

    /// <summary>
    ///     Determines whether the specified method symbol should be included.
    /// </summary>
    /// <param name="methodSymbol">The method symbol to check.</param>
    /// <returns><c>true</c> if the method symbol should be included; otherwise, <c>false</c>.</returns>
    private static bool Include(IMethodSymbol methodSymbol) =>
        methodSymbol.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected && !methodSymbol.IsStatic;

    /// <summary>
    ///     Builds the factory method for the specified symbol.
    /// </summary>
    /// <param name="builder">The code builder.</param>
    /// <param name="details">Details on the mock to build.</param>
    /// <param name="constructor">The constructor symbol, if any.</param>
    private static void BuildFactoryMethod(this CodeBuilder builder, MockDetails details, IMethodSymbol? constructor = null)
    {
        if (details.Target.TypeArguments.Length > 0)
            BuildGenericFactoryMethod(details, builder, constructor);
        else
            BuildNonGenericFactoryMethod(details, builder, constructor);
    }

    /// <summary>
    ///     Builds a non-generic factory method for the specified symbol.
    /// </summary>
    /// <param name="details">Details on the mock to build.</param>
    /// <param name="builder">The code builder.</param>
    /// <param name="constructor">The constructor symbol, if any.</param>
    private static void BuildNonGenericFactoryMethod(MockDetails details, CodeBuilder builder, IMethodSymbol? constructor)
    {
        var constructorParameters = constructor?.Parameters ?? [];

        var parameters = constructorParameters.ToString(t => $"{t.Type} {t.Name}, ", "");
        var names = constructorParameters.ToString(t => $"{t.Name}, ", "");

        var symbolName = details.Target.Name;

        var cref = details.Target.ToCRef();

        builder.Documentation(doc => doc
            .Summary($"Creates a mock object for <see cref=\"{cref}\"/>.")
            .Parameter(constructorParameters, t => t.Name, t => $"Base constructor parameter {t.Name}.")
            .Parameter("config", "Optional configuration for the mock object.")
            .Returns($"The mock object for <see cref=\"{cref}\"/>.")
        );

        builder.AddLines($"""
                      internal static {details.SourceName} {symbolName}
                          ({parameters}System.Action<{details.Namespace}.{details.MockType}.Config>? config = null)
                          => new {details.Namespace}.{details.MockName}({names}config);
                      """);

        builder.Documentation(doc => doc
            .Summary($"Creates a mock object for <see cref=\"{cref}\"/>.")
            .Parameter(constructorParameters, t => t.Name, t => $"Base constructor parameter {t.Name}.")
            .Parameter($"config{symbolName}", "Outputs configuration for the mock object.")
            .Returns($"The mock object for <see cref=\"{cref}\"/>."));

        builder.AddLines($$"""
                      internal static {{details.SourceName}} {{symbolName}}
                          ({{parameters}}out {{details.Namespace}}.{{details.MockType}}.Config config{{symbolName}})
                          {
                             {{details.Namespace}}.{{details.MockName}}.Config outConfig = null!;
                             var result = new {{details.Namespace}}.{{details.MockName}}({{names}}config => outConfig = config);
                             config{{symbolName}} = outConfig;
                             return result;
                          }
                      """);
    }

    /// <summary>
    ///     Builds a generic factory method for the specified symbol.
    /// </summary>
    /// <param name="details">Details on the mock to build.</param>
    /// <param name="builder">The code builder.</param>
    /// <param name="constructor">The constructor symbol, if any.</param>
    private static void BuildGenericFactoryMethod(MockDetails details, CodeBuilder builder, IMethodSymbol? constructor = null)
    {
        var constructorParameters = constructor?.Parameters ?? [];

        var parameters = constructorParameters.ToString(t => $"{t.Type} {t.Name}, ", "");
        var arguments = constructorParameters.ToString(t => $"{t.Name}, ", "");

        var cref = details.Target.ToCRef();

        var types = details.Target.TypeArguments.ToString(t => t.Name);
        var constraints = details.Target.TypeArguments.ToConstraints();

        builder.Documentation(doc => doc
            .Summary($"Creates a mock object for <see cref=\"{cref}\"/>.")
            .Parameter(constructorParameters, t => t.Name, t => $"Base constructor parameter {t.Name}.")
            .Parameter("config", "Optional configuration for the mock object.")
            .Returns($"The mock object for <see cref=\"{cref}\"/>."));

        builder.AddLines($"""
                      internal static {details.SourceName} {details.Target.Name}<{types}>
                          ({parameters}System.Action<{details.Namespace}.{details.MockType}.Config>? config = null) {constraints}
                          => new {details.Namespace}.{details.MockType}({arguments}config);
                      """);

        builder.Documentation(doc => doc
            .Summary($"Creates a mock object for <see cref=\"{cref}\"/>.")
            .Parameter(constructorParameters, t => t.Name, t => $"Base constructor parameter {t.Name}.")
            .Parameter("config", "Outputs configuration for the mock object.")
            .Returns($"The mock object for <see cref=\"{cref}\"/>."));

        builder.AddLines($$"""
                      internal static {{details.SourceName}} {{details.Target.Name}}<{{types}}>
                          ({{parameters}}out {{details.Namespace}}.{{details.MockType}}.Config config{{details.Target.Name}}) {{constraints}}
                          {
                             {{details.Namespace}}.{{details.MockType}}.Config outConfig = null!;
                             var result = new {{details.Namespace}}.{{details.MockType}}({{arguments}}config => outConfig = config);
                             config{{details.Target.Name}} = outConfig;;
                             return result;
                          }
                      """);
    }
}
