namespace SweetMock.Builders;

using System.Linq;
using Microsoft.CodeAnalysis;
using Utils;

/// <summary>
///     Provides methods to build mock classes.
/// </summary>
public class FactoryClassBuilder
{
    /// <summary>
    ///     Builds the mock classes based on the provided type symbols.
    /// </summary>
    /// <returns>A string containing the generated mock classes.</returns>
    public static string Build(MockDetails details)
    {
        var builder = new CodeBuilder();

        builder.AddFileHeader();
        builder.Add("""
                    #nullable enable
                    namespace SweetMock {
                    """).Indent();
        builder.AddSummary("Factory for creating mock objects.");
        builder.Add("internal static partial class Mock {").Indent();

        if (!details.Target.Constructors.Any(t => !t.IsStatic))
            BuildFactoryMethod(details, builder);
        else
            foreach (var constructor in details.Target.Constructors.Where(Include))
                BuildFactoryMethod(details, builder, constructor);

        builder.Unindent().Add("}").Unindent().Add("}");

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
    /// <param name="details">Details on the mock to build.</param>
    /// <param name="builder">The code builder.</param>
    /// <param name="constructor">The constructor symbol, if any.</param>
    private static void BuildFactoryMethod(MockDetails details, CodeBuilder builder, IMethodSymbol? constructor = null)
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

        builder.AddSummary($"Creates a mock object for <see cref=\"{cref}\"/>.");
        foreach (var parameter in constructorParameters)
        {
            builder.AddParameter(parameter.Name, $"Base constructor parameter {parameter.Name}.");
        }
        builder.AddParameter("config", "Optional configuration for the mock object.");
        builder.AddReturns($"The mock object for <see cref=\"{cref}\"/>.");
        builder.Add($"""
                      internal static {details.SourceName} {symbolName}
                          ({parameters}System.Action<{details.Namespace}.{details.MockType}.Config>? config = null)
                          => new {details.Namespace}.{details.MockName}({names}config);
                      """);

        builder.AddSummary($"Creates a mock object for <see cref=\"{cref}\"/>.");
        foreach (var parameter in constructorParameters)
        {
            builder.AddParameter(parameter.Name, $"Base constructor parameter {parameter.Name}.");
        }
        builder.AddParameter($"config{symbolName}", "Outputs configuration for the mock object.");
        builder.AddReturns($"The mock object for <see cref=\"{cref}\"/>.");
        builder.Add($$"""
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

        builder.AddSummary($"Creates a mock object for <see cref=\"{cref}\"/>.");
        foreach (var parameter in constructorParameters)
        {
            builder.AddParameter(parameter.Name, $"Base constructor parameter {parameter.Name}.");
        }
        builder.AddParameter("config", "Optional configuration for the mock object.");
        builder.AddReturns($"The mock object for <see cref=\"{cref}\"/>.");

        builder.Add($"""
                      internal static {details.SourceName} {details.Target.Name}<{types}>
                          ({parameters}System.Action<{details.Namespace}.{details.MockType}.Config>? config = null) {constraints}
                          => new {details.Namespace}.{details.MockType}({arguments}config);
                      """);

        builder.AddSummary($"Creates a mock object for <see cref=\"{cref}\"/>.");
        foreach (var parameter in constructorParameters)
        {
            builder.AddParameter(parameter.Name, $"Base constructor parameter {parameter.Name}.");
        }
        builder.AddParameter("config", "Outputs configuration for the mock object.");
        builder.AddReturns($"The mock object for <see cref=\"{cref}\"/>.");
        builder.Add($$"""
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
