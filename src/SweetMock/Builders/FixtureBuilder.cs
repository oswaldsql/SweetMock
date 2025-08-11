namespace SweetMock.Builders;

using Exceptions;
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
            var infos = mockInfos.ToDictionary(t => t.Target, NamedSymbolEqualityComparer.Default);
            var symbol = (INamedTypeSymbol)source;
            var fileScope = new CodeBuilder();
            var targetCtor = symbol.Constructors.First();

            fileScope.AddFileHeader()
                .Add("#nullable enable")
                .Scope($"namespace {symbol.ContainingNamespace}", namespaceScope => namespaceScope
                    .AddGeneratedCodeAttrib()
                    .Scope($"internal class FixtureFor_{symbol.Name}", classScope => classScope
                        .AddFixtureConfigObject(targetCtor, symbol, infos)
                        //.AddFixtureConfigObject2(targetCtor, symbol, infos)
                        .AddPrivateMockObjects(targetCtor, infos)
                        .AddCallLog()
                        .AddConstructor(symbol, targetCtor, infos)
                        .AddCreateSutMethod(targetCtor, symbol, infos)));

            return fileScope.ToString();
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    private static CodeBuilder AddFixtureConfigObject(this CodeBuilder classScope, IMethodSymbol targetCtor, INamedTypeSymbol s, Dictionary<INamedTypeSymbol, MockInfo> infos)
    {
        classScope.Documentation(builder =>
        {
            builder.Summary("Configuration object for the fixture");
            foreach (var parameter in targetCtor.Parameters)
            {
                builder.Parameter(parameter.Name, $"Configuring the {parameter.Name} ({parameter.Type.ToSeeCRef()}) mock for the fixture {s.ToSeeCRef()}.");
            }
        });

        var configParameters = string.Join(", ", BuildFixtureConfigParameters(targetCtor, infos));
        classScope
            .Add($"internal record FixtureConfig({configParameters});")
            .AddLineBreak();

        classScope
            .Documentation(d => d
                .Summary("Gets or sets the configuration object for the fixture used in the test setup process.", "This property enabled configuration and management of the mocked dependencies."))
            .Add("internal FixtureConfig Config{get; private set;}")
            .AddLineBreak();

        return classScope;
    }

    private static CodeBuilder AddFixtureConfigObject2(this CodeBuilder classScope, IMethodSymbol targetCtor, INamedTypeSymbol s, Dictionary<INamedTypeSymbol, MockInfo> infos)
    {
        classScope.Documentation(builder =>
        {
            builder.Summary("Configuration object for the fixture");
            foreach (var parameter in targetCtor.Parameters)
            {
                builder.Parameter(parameter.Name, $"Configuring the {parameter.Name} ({parameter.Type.ToSeeCRef()}) mock for the fixture {s.ToSeeCRef()}.");
            }
        });

        var configParameters = string.Join(", ", BuildFixtureConfigParameters(targetCtor, infos));
        classScope
            .Scope($"internal class FixtureConfig2", classScope =>
            {
                classScope.Scope($"private FixtureConfig2({configParameters})", ctorScope =>
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
                    var mockType = GetMockType(infos, parameter);
                    switch (mockType)
                    {
                        case MockType.Generated:
                            break;
                        case MockType.Wrapper:
                            break;
                        case MockType.Direct:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    if (infos.TryGetValue(parameter.Type.OriginalDefinition as INamedTypeSymbol, out var info))
                    {
                        classScope.Add($"internal {parameter.Type} {parameter.Name} {{get;private set;}}");// yield return $"{info.MockClass}{generics}.Config {parameter.Name}";
                    }
                    else
                    {
                        classScope.Add($"internal {parameter.Type} {parameter.Name} {{get; set;}}");// yield return $"{parameter.Type} {parameter.Name}";
                    }
                }
            })
            .AddLineBreak();

        classScope
            .Documentation(d => d
                .Summary("Gets or sets the configuration object for the fixture used in the test setup process.", "This property enabled configuration and management of the mocked dependencies."))
            .Add("internal FixtureConfig2 Config2{get; private set;}")
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
                classScope.Add($"private readonly {parameterInfo.MockClass}{generics} _{parameter.Name};");
            }
        }

        classScope.AddLineBreak();

        return classScope;
    }

    private static CodeBuilder AddCallLog(this CodeBuilder classScope) =>
        classScope
            .Documentation(d => d.Summary("Gets the call log used to record method invocations and interactions within the mocked dependencies during the test execution process.", "This property facilitates the tracking and validation of method calls made on the mocks in the scope of the unit tests."))
            .Add("public SweetMock.CallLog Log{get; private set;}")
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
                        ctorScope.Add($"{parameter.Type} temp_{parameter.Name} = default;").AddLineBreak();
                    }
                    else
                    {
                        var generics = type.GetTypeGenerics();
                        ctorScope
                            .Add($"{parameterInfo.MockClass}{generics}.Config temp_{parameter.Name} = null!;")
                            .Add($"_{parameter.Name} = new {parameterInfo.MockClass}{generics}(config => temp_{parameter.Name} = config, new SweetMock.MockOptions(Log, \"{parameter.Name}\"));")
                            .AddLineBreak();
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
        classScope
            .Documentation(d => d
                .Summary($"Creates an instance of the {s.ToSeeCRef()} object using the initialized mock dependencies.")
                .Returns($"A {s.ToSeeCRef()} instance configured with mocked dependencies.")
            )
            .Add($"public {s} CreateSut() =>").Indent()
            .Add($"new {s.Name}({parametersString});")
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
        var mockType = GetMockType(infos, t);
        return mockType switch
        {
            MockType.Wrapper => $"_{t.Name}.Value",
            MockType.Generated => $"_{t.Name}",
            MockType.Direct => $"Config.{t.Name}",
            _ => ""
        };
    }

    private static MockType GetMockType(Dictionary<INamedTypeSymbol, MockInfo> infos, IParameterSymbol t)
    {
        if (infos.TryGetValue(t.Type.OriginalDefinition as INamedTypeSymbol, out var result))
        {
            return result.Type;
        }
        else
        {
            return MockType.Direct;
        }
    }

    private static IEnumerable<string> BuildFixtureConfigParameters(IMethodSymbol targetCtor, Dictionary<INamedTypeSymbol, MockInfo> infos)
    {
        foreach (var parameter in targetCtor.Parameters)
        {
            var type = (INamedTypeSymbol)parameter.Type;
            var generics = type.GetTypeGenerics();
            if (infos.TryGetValue(parameter.Type.OriginalDefinition as INamedTypeSymbol, out var info))
            {
                yield return $"{info.MockClass}{generics}.Config {parameter.Name}";
            }
            else
            {
                yield return $"{parameter.Type} {parameter.Name}";
            }
        }
    }

    public static string BuildFixturesFactory(IEnumerable<INamedTypeSymbol> source)
    {
        var fileScope = new CodeBuilder();
        fileScope.AddFileHeader()
            .Add("#nullable enable")
            .Scope("namespace SweetMock", namespaceScope =>
                namespaceScope
                    .Scope("internal static partial class Fixture", classScope =>
                    {
                        foreach (var symbol in source)
                        {
                            BuildForFixture(classScope, symbol);
                        }
                    }));

        return fileScope.ToString();
    }

    private static CodeBuilder BuildForFixture(CodeBuilder classScope, INamedTypeSymbol symbol) =>
        classScope
            .Documentation(d => d
                .Summary($"Represents a test fixture designed for the {symbol.ToSeeCRef()} class, leveraging mocked dependencies for unit testing.")
                .Parameter("config", "An optional configuration action to customize the mocked dependencies or fixture setup.")
                .Returns($"Returns a fixture object configured for testing the {symbol.ToSeeCRef()} class.")
            )
            .Scope($"public static {symbol.ContainingNamespace}.FixtureFor_{symbol.Name} {symbol.Name}(System.Action<{symbol.ContainingNamespace}.FixtureFor_{symbol.Name}.FixtureConfig>? config = null)", methodScope =>
                methodScope
                    .Add($"var result = new {symbol.ContainingNamespace}.FixtureFor_{symbol.Name}(config);")
                    .Add("return result;"));
}
