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
            .Nullable()
            .Add("namespace SweetMock;")
            .Documentation("Factory for creating mock objects.")
            .AddGeneratedCodeAttrib()
            .Scope("internal static class Mock", mockScope =>
            {
                foreach (var mock in mocks)
                {
                    mockScope.Region(mock.Key!.ToCRef(), regionScope =>
                    {
                        var mockInfo = mock.First();
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
                                BuildCustomMockFactory(regionScope, mockInfo.Source, mockInfo.Implementation!);
                                break;
                            case MockKind.BuildIn:
                                BuildGeneratedFactory(regionScope, mockInfo);
                                break;
                            case MockKind.Direct:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    });
                }
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
    ///     Builds a generic factory method for the specified symbol.
    /// </summary>
    /// <param name="builder">The code builder.</param>
    /// <param name="mockInfo"></param>
    /// <param name="constructor">The constructor symbol, if any.</param>
    private static void BuildGeneratedFactory(CodeBuilder builder, MockInfo mockInfo, IMethodSymbol? constructor = null)
    {
        var constructorParameters = constructor?.Parameters ?? [];

        var parameters = constructorParameters.Combine(t => $"{t.Type} {t.Name}, ", "");
        var arguments = constructorParameters.Combine(t => $"{t.Name}, ", "");

        builder
            .Documentation(doc => doc
                .Summary($"Creates a mock object for {mockInfo.ToSeeCRef}.")
                .Parameters(constructorParameters, t => $"Base constructor parameter {t.Name}.")
                .Parameter("config", "Optional configuration for the mock object.")
                .Parameter("options", "Options for the mock object.")
                .Returns($"The mock object for {mockInfo.ToSeeCRef}."))
            .Add($"internal static {mockInfo.FullName} {mockInfo.Name}{mockInfo.Generics}")
            .Scope($"({parameters}System.Action<{mockInfo.Namespace}.{mockInfo.MockType}.{mockInfo.ContextConfigName}>? config = null, global::SweetMock.MockOptions? options = null) {mockInfo.Constraints}", methodScope => methodScope
                .Add($"return new {mockInfo.Namespace}.{mockInfo.MockType}({arguments}config, options);"));

        builder.BR();

        builder
            .Documentation(doc => doc
                .Summary($"Creates a mock object for {mockInfo.ToSeeCRef}.")
                .Parameters(constructorParameters, t => $"Base constructor parameter {t.Name}.")
                .Parameter($"config{mockInfo.Name}", "Outputs configuration for the mock object.")
                .Parameter("options", "Options for the mock object.")
                .Returns($"The mock object for {mockInfo.ToSeeCRef}."))
            .Add($"internal static {mockInfo.FullName} {mockInfo.Name}{mockInfo.Generics}")
            .Scope($"({parameters}out {mockInfo.Namespace}.{mockInfo.MockType}.{mockInfo.ContextConfigName} config{mockInfo.Name}, global::SweetMock.MockOptions? options = null) {mockInfo.Constraints}", methodScope => methodScope
                .Add($"{mockInfo.Namespace}.{mockInfo.MockType}.{mockInfo.ContextConfigName} outConfig = null!;")
                .Add($"var result = new {mockInfo.Namespace}.{mockInfo.MockType}({arguments}config => outConfig = config, options);")
                .Add($"config{mockInfo.Name} = outConfig;;")
                .Add("return result;"));
    }

    private static void BuildCustomMockFactory(CodeBuilder mockScope, INamedTypeSymbol type, INamedTypeSymbol implementationType)
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
}
