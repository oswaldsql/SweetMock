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
            .Nullable();

        builder.Scope("namespace SweetMock", namespaceScope => namespaceScope
            .Documentation("Factory for creating mock objects.")
            .AddGeneratedCodeAttrib()
            .Scope("internal static class Mock", mockScope =>
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
                            case MockKind.BuildIn:
                                BuildBuildInMockFactory(mockInfo, mockScope);
                                break;
                            case MockKind.Direct:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
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
        var generics = source.GetTypeGenerics();
        var detailsMockType = "MockOf_" + source.Name + generics;
        var detailsSourceName = source.ToString();

        var constructorParameters = constructor?.Parameters ?? [];

        var parameters = constructorParameters.ToString(t => $"{t.Type} {t.Name}, ", "");
        var arguments = constructorParameters.ToString(t => $"{t.Name}, ", "");

        var constraints = source.ToConstraints();

        builder
            .Documentation(doc => doc
                .Summary($"Creates a mock object for {source.ToSeeCRef()}.")
                .Parameter(constructorParameters, t => $"Base constructor parameter {t.Name}.")
                .Parameter("config", "Optional configuration for the mock object.")
                .Parameter("options", "Options for the mock object.")
                .Returns($"The mock object for {source.ToSeeCRef()}."))
            .Add($"internal static {detailsSourceName} {source.Name}{generics}")
            .Scope($"({parameters}System.Action<{detailsNamespace}.{detailsMockType}.{mockInfo.ContextConfigName}>? config = null, global::SweetMock.MockOptions? options = null) {constraints}", methodScope => methodScope
                .Add($"return new {detailsNamespace}.{detailsMockType}({arguments}config, options);"));

        builder.BR();

        builder
            .Documentation(doc => doc
                .Summary($"Creates a mock object for {source.ToSeeCRef()}.")
                .Parameter(constructorParameters, t => $"Base constructor parameter {t.Name}.")
                .Parameter($"config{source.Name}", "Outputs configuration for the mock object.")
                .Parameter("options", "Options for the mock object.")
                .Returns($"The mock object for {source.ToSeeCRef()}."))
            .Add($"internal static {detailsSourceName} {source.Name}{generics}")
            .Scope($"({parameters}out {detailsNamespace}.{detailsMockType}.{mockInfo.ContextConfigName} config{source.Name}, global::SweetMock.MockOptions? options = null) {constraints}", methodScope => methodScope
                .Add($"{detailsNamespace}.{detailsMockType}.{mockInfo.ContextConfigName} outConfig = null!;")
                .Add($"var result = new {detailsNamespace}.{detailsMockType}({arguments}config => outConfig = config, options);")
                .Add($"config{source.Name} = outConfig;;")
                .Add("return result;"));
    }

    private static void BuildCustomMockFactory(INamedTypeSymbol type, INamedTypeSymbol implementationType, CodeBuilder mockScope)
    {
        var generics = type.GetTypeGenerics();
        var constraints = type.ToConstraints();

        mockScope
            .Documentation(doc => doc
                .Summary($"Creates a mock object for {type.ToSeeCRef()}.")
                .Parameter("config", "Optional configuration for the mock object.")
                .Parameter("options", "Options for the mock object.")
                .Returns($"The mock object for {type.ToSeeCRef()}."))
            .Add($"internal static {implementationType} {type.Name}{generics}")
            .Scope($"(global::System.Action<{implementationType}.MockConfig>? config = null, global::SweetMock.MockOptions? options = null){constraints}", methodScope => methodScope
                .Add($"var result = new {implementationType}();")
                .Add("config?.Invoke(result.Config);")
                .Add("result.Options = options ?? result.Options;")
                .Add("return result;")
            );

        mockScope.BR();

        mockScope
            .Documentation(doc => doc
                .Summary($"Creates a mock object for {type.ToSeeCRef()}.")
                .Parameter($"config{type.Name}", "Outputs configuration for the mock object.")
                .Parameter("options", "Options for the mock object.")
                .Returns($"The mock object for {type.ToSeeCRef()}."))
            .Add($"internal static {implementationType} {type.Name}{generics}")
            .Scope($"(out {implementationType}.MockConfig config{type.Name}, global::SweetMock.MockOptions? options = null){constraints}", methodScope => methodScope
                .Add($"var result = new {implementationType}();")
                .Add($"config{type.Name} = result.Config;")
                .Add("result.Options = options ?? result.Options;")
                .Add("return result;"));
    }

    private static void BuildBuildInMockFactory(MockInfo mockInfo, CodeBuilder mockScope)
    {
        var generics = mockInfo.Source.GetTypeGenerics();
        var constraints = mockInfo.Source.ToConstraints();

        mockScope
            .Documentation(doc => doc
                .Summary($"Creates a mock object for {mockInfo.Source.ToSeeCRef()}.")
                .Parameter("config", "Optional configuration for the mock object.")
                .Parameter("options", "Options for the mock object.")
                .Returns($"The mock object for {mockInfo.Source.ToSeeCRef()}."))
            .Add($"internal static {mockInfo.MockClass}{generics} {mockInfo.Source.Name}{generics}")
            .Scope($"(global::System.Action<{mockInfo.MockClass}{generics}.{mockInfo.ContextConfigName}>? config = null, global::SweetMock.MockOptions? options = null){constraints}", methodScope => methodScope
                .Add($"var result = new {mockInfo.MockClass}{generics}();")
                .Add("config?.Invoke(result.Config);")
                .Add("result.Options = options ?? result.Options;")
                .Add("return result;")
            );

        mockScope.BR();
    }
}
