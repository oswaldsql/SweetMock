namespace SweetMock.Builders;

using Generation;
using Utils;

public static class FixtureBuilder
{
    public static IEnumerable<ITypeSymbol> GetRequiredMocks(INamedTypeSymbol symbol)
    {
        var targetCtor = symbol.Constructors.First();
        foreach (var param in targetCtor.Parameters)
        {
            if (param.Type is INamedTypeSymbol paramSymbol)
            {
                yield return paramSymbol.IsGenericType ? paramSymbol.OriginalDefinition : paramSymbol;
            }
        }
    }

    public static string BuildFixture(ISymbol source, List<MockInfo> mockInfos)
    {
        try
        {
            var infos = mockInfos.ToDictionary(t => t.Source, NamedSymbolEqualityComparer.Default);
            var symbol = (INamedTypeSymbol)source;
            var fileScope = new CodeBuilder();
            var targetCtor = symbol.Constructors.First();
            var generics = symbol.GetTypeGenerics();
            var constraints = symbol.ToConstraints();

            fileScope.AddFileHeader()
                .Nullable()
                .Scope($"namespace {symbol.ContainingNamespace}", namespaceScope => namespaceScope
                    .AddGeneratedCodeAttrib()
                    .Scope($"internal class FixtureFor_{symbol.Name}{generics}{constraints}", classScope => classScope
                        .AddFixtureConfigObject(targetCtor, symbol, infos)
                        .AddPrivateMockObjects(targetCtor, infos)
                        .AddCallLog()
                        .AddConstructor(symbol, targetCtor, infos)
                        .AddCreateSutMethod(targetCtor, symbol, infos)
                        .End()));

            return fileScope.ToString();
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    private static CodeBuilder AddFixtureConfigObject(this CodeBuilder classScope, IMethodSymbol targetCtor, INamedTypeSymbol s, Dictionary<INamedTypeSymbol, MockInfo> infos)
    {
        var configParameters = string.Join(", ", BuildFixtureConfigParameters(targetCtor, infos));
        classScope
            .Scope($"internal class FixtureConfig", configScope =>
            {
                configScope.Documentation(builder =>
                    {
                        builder.Summary("Configuration object for the fixture");
                        builder.Parameter(targetCtor.Parameters, p => p.Name, p => $"Configuring the {p.Name} ({p.Type.ToSeeCRef()}) mock for the fixture {s.ToSeeCRef()}.");
                    })
                    .Scope($"internal FixtureConfig({configParameters})", ctorScope =>
                    {
                        foreach (var parameter in targetCtor.Parameters)
                        {
                            ctorScope.Add($"this.{parameter.Name} = {parameter.Name};");
                        }
                    });

                foreach (var parameter in targetCtor.Parameters)
                {
                    var type = (INamedTypeSymbol)parameter.Type;
                    var generics = type.GetTypeGenerics();
                    if (infos.TryGetValue((INamedTypeSymbol)parameter.Type.OriginalDefinition, out var info))
                    {
                        configScope
                            .AddLineBreak()
                            .Documentation(doc => doc
                                .Summary($"Gets the configuration for {parameter.Name} used within the fixture."))
                            .Add($"internal global::{info.MockClass}{generics}.{info.ContextConfigName} {parameter.Name} {{get;private set;}}");
                    }
                    else
                    {
                        configScope
                            .AddLineBreak()
                            .Documentation(doc => doc
                                .Summary($"Gets or sets the {parameter.Name} used for configuration within the fixture."))
                            .Add($"internal {parameter.Type.ToDisplayString(ToFullNameFormat)}{generics}? {parameter.Name} {{get; set;}}");
                    }
                }
            })
            .AddLineBreak();

        classScope
            .Documentation(d => d
                .Summary("Gets or sets the configuration object for the fixture used in the test setup process.", "This property enabled configuration and management of the mocked dependencies."))
            .Add("internal FixtureConfig Config{get; private set;}")
            .AddLineBreak();

        return classScope;
    }

    private static CodeBuilder AddPrivateMockObjects(this CodeBuilder classScope, IMethodSymbol targetCtor, Dictionary<INamedTypeSymbol, MockInfo> infos)
    {
        foreach (var parameter in targetCtor.Parameters)
        {
            var type = parameter.Type as INamedTypeSymbol;
            if (infos.TryGetValue(type!.OriginalDefinition, out var parameterInfo))
            {
                var generics = type.GetTypeGenerics();
                classScope.Add($"private readonly global::{parameterInfo.MockClass}{generics} _{parameter.Name};");
            }
        }

        classScope.AddLineBreak();

        return classScope;
    }

    private static CodeBuilder AddCallLog(this CodeBuilder classScope) =>
        classScope
            .Documentation(d => d.Summary("Gets the call log used to record method invocations and interactions within the mocked dependencies during the test execution process.", "This property facilitates the tracking and validation of method calls made on the mocks in the scope of the unit tests."))
            .Add("public global::SweetMock.CallLog Log{get; private set;}")
            .AddLineBreak();

    private static CodeBuilder AddConstructor(this CodeBuilder classScope, INamedTypeSymbol s, IMethodSymbol targetCtor, Dictionary<INamedTypeSymbol, MockInfo> infos) =>
        classScope
            .Documentation(d => d
                .Summary($"Provides a fixture for the {s.ToSeeCRef()} object, setting up mocks and a call log for testing purposes.")
                .Parameter("config", "Optional configuration of the mocked dependencies.")
            )
            .Scope($"public FixtureFor_{s.Name}(System.Action<FixtureConfig>? config = null)", ctorScope =>
            {
                ctorScope.Add("Log = new SweetMock.CallLog();").AddLineBreak();

                foreach (var parameter in targetCtor.Parameters)
                {
                    var type = parameter.Type as INamedTypeSymbol;
                    if (!infos.TryGetValue(type!.OriginalDefinition, out var parameterInfo))
                    {
                        ctorScope.Add($"{parameter.Type.ToDisplayString(ToFullNameFormat)}? temp_{parameter.Name} = default;").AddLineBreak();
                    }
                    else
                    {
                        var generics = type.GetTypeGenerics();
                        if (parameterInfo.Kind is MockKind.Wrapper or MockKind.BuildIn)
                        {
                            ctorScope
                                .Add($"_{parameter.Name} = new global::{parameterInfo.MockClass}{generics}();")
                                .Add($"var temp_{parameter.Name} = _{parameter.Name}.Config;")
                                .Add($"_{parameter.Name}.Options = new global::SweetMock.MockOptions(Log, \"{parameter.Name}\");")
                                .AddLineBreak();
                        }
                        else
                        {
                            ctorScope
                                .Add($"global::{parameterInfo.MockClass}{generics}.{parameterInfo.ContextConfigName} temp_{parameter.Name} = null!;")
                                .Add($"_{parameter.Name} = new {parameterInfo.MockClass}{generics}(config => temp_{parameter.Name} = config, new SweetMock.MockOptions(Log, \"{parameter.Name}\"));")
                                .AddLineBreak();
                        }
                    }
                }

                var parametersString = targetCtor.Parameters.ToString(t => "temp_" + t.Name);
                classScope
                    .Add($"Config = new FixtureConfig({parametersString});")
                    .Add("config?.Invoke(Config);");
            }).AddLineBreak();

    private static CodeBuilder AddCreateSutMethod(this CodeBuilder classScope, IMethodSymbol targetCtor, INamedTypeSymbol s, Dictionary<INamedTypeSymbol, MockInfo> infos)
    {
        var parametersString = BuildParametersString(targetCtor, infos);
        var generics = s.GetTypeGenerics();

        classScope
            .Documentation(d => d
                .Summary($"Creates an instance of the {s.ToSeeCRef()} object using the initialized mock dependencies.")
                .Returns($"A {s.ToSeeCRef()} instance configured with mocked dependencies.")
            )
            .Add($"public {s.ToDisplayString(ToFullNameFormat)} CreateSut() =>").Indent()
            .Add($"new {s.ToDisplayString(ToFullNameFormat)}({parametersString});")
            .Unindent();

        return classScope;
    }

    private static string BuildParametersString(IMethodSymbol targetCtor, Dictionary<INamedTypeSymbol, MockInfo> infos)
    {
        var arguments = targetCtor.Parameters.Select(t => MockTypeToArgument(infos, t));
        return string.Join(", ", arguments);
    }

    private static string MockTypeToArgument(Dictionary<INamedTypeSymbol, MockInfo> infos, IParameterSymbol t)
    {
        var canBeNull = t.Type.NullableAnnotation == NullableAnnotation.Annotated;
        var mockType = GetMockType(infos, t);
        return mockType switch
        {
            MockKind.Wrapper => $"_{t.Name}.Value",
            MockKind.BuildIn => $"_{t.Name}.Value",
            MockKind.Generated => $"_{t.Name}",
            MockKind.Direct when canBeNull => $"Config.{t.Name}",
            MockKind.Direct when !canBeNull => $"Config.{t.Name} ?? throw new NullReferenceException()",
            _ => ""
        };
    }

    private static MockKind GetMockType(Dictionary<INamedTypeSymbol, MockInfo> infos, IParameterSymbol t)
    {
        if (infos.TryGetValue((INamedTypeSymbol)t.Type.OriginalDefinition, out var result))
        {
            return result.Kind;
        }
        else
        {
            return MockKind.Direct;
        }
    }

    private static IEnumerable<string> BuildFixtureConfigParameters(IMethodSymbol targetCtor, Dictionary<INamedTypeSymbol, MockInfo> infos)
    {
        foreach (var parameter in targetCtor.Parameters)
        {
            var type = (INamedTypeSymbol)parameter.Type;
            var generics = type.GetTypeGenerics();
            if (infos.TryGetValue((INamedTypeSymbol)parameter.Type.OriginalDefinition, out var info))
            {
                yield return $"global::{info.MockClass}{generics}.{info.ContextConfigName} {parameter.Name}";
            }
            else
            {
                yield return $"{parameter.Type.ToDisplayString(ToFullNameFormat)}? {parameter.Name}";
            }
        }
    }

    private static readonly SymbolDisplayFormat ToFullNameFormat = new(
        SymbolDisplayGlobalNamespaceStyle.Included,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        SymbolDisplayGenericsOptions.IncludeTypeParameters,
        memberOptions: SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeContainingType,
        parameterOptions: SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType
    );

    public static string BuildFixturesFactory(IEnumerable<INamedTypeSymbol> source)
    {
        var fileScope = new CodeBuilder();
        fileScope.AddFileHeader()
            .Nullable()
            .Scope("namespace SweetMock", namespaceScope =>
                namespaceScope
                    .AddGeneratedCodeAttrib()
                    .Scope("internal static class Fixture", classScope =>
                    {
                        foreach (var symbol in source)
                        {
                            BuildForFixture(classScope, symbol);
                        }
                    }));

        return fileScope.ToString();
    }

    private static void BuildForFixture(CodeBuilder classScope, INamedTypeSymbol symbol)
    {
        var generics = symbol.GetTypeGenerics();
        var constraints = symbol.ToConstraints();

        classScope
            .Documentation(d => d
                .Summary($"Represents a test fixture designed for the {symbol.ToSeeCRef()} class, leveraging mocked dependencies for unit testing.")
                .Parameter("config", "An optional configuration action to customize the mocked dependencies or fixture setup.")
                .Returns($"Returns a fixture object configured for testing the {symbol.ToSeeCRef()} class.")
            )
            .Scope($"public static {symbol.ContainingNamespace}.FixtureFor_{symbol.Name}{generics} {symbol.Name}{generics}(System.Action<{symbol.ContainingNamespace}.FixtureFor_{symbol.Name}{generics}.FixtureConfig>? config = null){constraints}", methodScope =>
                methodScope
                    .Add($"var result = new {symbol.ContainingNamespace}.FixtureFor_{symbol.Name}{generics}(config);")
                    .Add("return result;"));
    }
}
