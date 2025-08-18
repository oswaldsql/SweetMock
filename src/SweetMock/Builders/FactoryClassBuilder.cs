namespace SweetMock.Builders;

using Generation;
using Utils;

/// <summary>
///     Provides methods to build mock classes.
/// </summary>
public static class FactoryClassBuilder
{
    internal static string Build(List<MockInfo> collectedMocks)
    {
        var mocks = collectedMocks.ToLookup(t => t.Source, SymbolEqualityComparer.Default);

        var builder = new CodeBuilder();

        builder
            .AddFileHeader()
            .Add("#nullable enable");

        builder.Scope("namespace SweetMock", namespaceScope => namespaceScope
            .Documentation(doc => doc
                .Summary("Factory for creating mock objects."))
            .Scope("internal static partial class Mock", mockScope =>
            {
                foreach (var t in mocks)
                {
                    mockScope.Region(t.Key!.ToCRef(), regionScope =>
                    {
                        var mockInfo = t.First();
                        switch (mockInfo.Kind)
                        {
                            case MockKind.Generated:
                            {
                                var constructors = mockInfo.Source.Constructors.Where(Include).ToArray();
                                foreach (var constructor in constructors)
                                {
                                    BuildGeneratedFactory(regionScope, mockInfo, constructor);
                                }

                                if (constructors.Length == 0)
                                {
                                    BuildGeneratedFactory(regionScope, mockInfo);
                                }

                                break;
                            }
                            case MockKind.Wrapper:
                                BuildCustomMockFactory(mockInfo.Source, mockInfo.Implementation!, mockScope);
                                break;
                        }
                    });
                }
            }));

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
    ///     Builds a generic factory method for the specified symbol.
    /// </summary>
    /// <param name="builder">The code builder.</param>
    /// <param name="mockInfo"></param>
    /// <param name="constructor">The constructor symbol, if any.</param>
    private static void BuildGeneratedFactory(CodeBuilder builder, MockInfo mockInfo, IMethodSymbol? constructor = null)
    {
        var source = mockInfo.Source;
        var detailsNamespace = source.ContainingNamespace;
        var generics = source.IsGenericType ? "<" + source.TypeArguments.ToString(t => t.Name) + ">" : "";
        var detailsMockType = "MockOf_" + source.Name + generics;
        var detailsSourceName = source.ToString();

        var constructorParameters = constructor?.Parameters ?? [];

        var parameters = constructorParameters.ToString(t => $"{t.Type} {t.Name}, ", "");
        var arguments = constructorParameters.ToString(t => $"{t.Name}, ", "");

        var constraints = source.TypeArguments.ToConstraints();

        builder
            .Documentation(doc => doc
                .Summary($"Creates a mock object for {source.ToSeeCRef()}.")
                .Parameter(constructorParameters, t => t.Name, t => $"Base constructor parameter {t.Name}.")
                .Parameter("config", "Optional configuration for the mock object.")
                .Parameter("options", "Options for the mock object.")
                .Returns($"The mock object for {source.ToSeeCRef()}."))
            .Add($"internal static {detailsSourceName} {source.Name}{generics}")
            .Scope($"({parameters}System.Action<{detailsNamespace}.{detailsMockType}.{mockInfo.contextConfigName}>? config = null, MockOptions? options = null) {constraints}", methodScope => methodScope
                .Add($"return new {detailsNamespace}.{detailsMockType}({arguments}config, options);"));

        builder.AddLineBreak();

        builder
            .Documentation(doc => doc
                .Summary($"Creates a mock object for {source.ToSeeCRef()}.")
                .Parameter(constructorParameters, t => t.Name, t => $"Base constructor parameter {t.Name}.")
                .Parameter($"config{source.Name}", "Outputs configuration for the mock object.")
                .Parameter("options", "Options for the mock object.")
                .Returns($"The mock object for {source.ToSeeCRef()}."))
            .Add($"internal static {detailsSourceName} {source.Name}{generics}")
            .Scope($"({parameters}out {detailsNamespace}.{detailsMockType}.{mockInfo.contextConfigName} config{source.Name}, MockOptions? options = null) {constraints}", methodScope => methodScope
                .Add($"{detailsNamespace}.{detailsMockType}.{mockInfo.contextConfigName} outConfig = null!;")
                .Add($"var result = new {detailsNamespace}.{detailsMockType}({arguments}config => outConfig = config, options);")
                .Add($"config{source.Name} = outConfig;;")
                .Add("return result;"));
    }

    private static void BuildCustomMockFactory(INamedTypeSymbol type, INamedTypeSymbol implementationType, CodeBuilder mockScope)
    {
        var generics = type.GetTypeGenerics();
        var constraints = type.TypeArguments.ToConstraints();

        mockScope
            .Documentation(doc => doc
                .Summary($"Creates a mock object for {type.ToSeeCRef()}.")
                .Parameter("config", "Optional configuration for the mock object.")
                .Parameter("options", "Options for the mock object.")
                .Returns($"The mock object for {type.ToSeeCRef()}."))
            .Add($"internal static {implementationType} {type.Name}{generics}")
            .Scope($"(System.Action<{implementationType}.Config>? config = null, MockOptions? options = null){constraints}", methodScope => methodScope
                .Add($"return new {implementationType}(config);"));

        mockScope.AddLineBreak();

        mockScope
            .Documentation(doc => doc
                .Summary($"Creates a mock object for {type.ToSeeCRef()}.")
                .Parameter($"config{type.Name}", "Outputs configuration for the mock object.")
                .Parameter("options", "Options for the mock object.")
                .Returns($"The mock object for {type.ToSeeCRef()}."))
            .Add($"internal static {implementationType} {type.Name}{generics}")
            .Scope($"(out {implementationType}.Config config{type.Name}, MockOptions? options = null){constraints}", methodScope => methodScope
                .Add($"{implementationType}.Config outConfig = null!;")
                .Add($"var result = new {implementationType}(config => outConfig = config);")
                .Add($"config{type.Name} = outConfig;")
                .Add("return result;"));
    }
}
