namespace SweetMock.Builders;

using Generation;
using Utils;

public static class FixtureBuilder
{
    public static IEnumerable<ITypeSymbol> GetRequiredMocks(ISymbol symbol)
    {
        var s = (INamedTypeSymbol)symbol;
        var targetCtor = s.Constructors.First();
        foreach (var param in targetCtor.Parameters)
        {
            if (param.Type is INamedTypeSymbol paramSymbol)
            {
                yield return paramSymbol.IsGenericType ? paramSymbol.OriginalDefinition : paramSymbol;
            }
        }

//        return targetCtor.Parameters.Select(t => t.Type);
    }

    public static string BuildFixture(ISymbol source)
    {
        var s = (INamedTypeSymbol)source;

        var fileScope = new CodeBuilder();

        var targetCtor = s.Constructors.First();

        fileScope.AddFileHeader()
            .Add("#nullable enable")
            .Scope($"namespace {s.ContainingNamespace}", namespaceScope =>
            {
                namespaceScope
                    .AddGeneratedCodeAttrib()
                    .Scope($"internal class FixtureFor_{s.Name}", classScope =>
                    {
                        var configString = string.Join(", ", targetCtor.Parameters.Select(parameter => $"{parameter.Type.ContainingNamespace}.MockOf_{parameter.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}.Config {parameter.Name}"));
                        classScope.Documentation(builder =>
                            {
                                builder.Summary("Configuration object for the fixture");
                                foreach (var parameter in targetCtor.Parameters)
                                {
                                    builder.Parameter(parameter.Name, $"Configuring the {parameter.Name} mock of type {s.ToSeeCRef()}.");
                                }
                            })
                            .Add($"internal record FixtureConfig({configString});")
                            .AddLineBreak();

                        classScope
                            .Documentation(d => d
                                .Summary("Gets or sets the configuration object for the fixture used in the test setup process.", "This property provides the capability to configure and manage the mocked dependencies"))
                            .Add("internal FixtureConfig Config{get; private set;}")
                            .AddLineBreak();

                        foreach (var parameter in targetCtor.Parameters)
                        {
                            classScope.Add($"private readonly {parameter.Type.ToDisplayString()} _{parameter.Name};");
                        }
                        classScope.AddLineBreak();

                        classScope
                            .Documentation(d => d.Summary("Gets the call log used to record method invocations and interactions within the mocked dependencies during the test execution process.","This property facilitates the tracking and validation of method calls made on the mocks in the scope of the unit tests."))
                            .Add("public SweetMock.CallLog Log{get; private set;}")
                            .AddLineBreak();

                        classScope
                            .Documentation(d => d
                                .Summary($"Provides a fixture for the {s.ToSeeCRef()} object, setting up mocks and a call log for testing purposes.")
                                .Parameter("config", "Optional configuration of the mocked dependencies.")
                            )
                            .Scope($"public FixtureFor_{s.Name}(System.Action<FixtureConfig>? config = null)", ctorScope =>
                        {
                            ctorScope.Add("Log = new SweetMock.CallLog();");

                            foreach (var parameter in targetCtor.Parameters)
                            {
                                ctorScope
                                    .Add($"_{parameter.Name} = SweetMock.Mock.{parameter.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}(out var temp_{parameter.Name}, new SweetMock.MockOptions(Log));");
                            }

                            var parametersString = string.Join(", ", targetCtor.Parameters.Select(t => "temp_" + t.Name));
                            classScope
                                .AddLineBreak()
                                .Add($"Config = new FixtureConfig({parametersString});")
                                .Add("config?.Invoke(Config);");
                        }).AddLineBreak();

                        var parametersString = string.Join(", ", targetCtor.Parameters.Select(t => "_" + t.Name));
                        classScope
                            .Documentation(d => d
                                .Summary($"Creates an instance of the {s.ToSeeCRef()} object using the initialized mock dependencies.")
                                .Returns($"A {s.ToSeeCRef()} instance configured with mocked dependencies.")
                            )
                            .Add($"public {s} CreateSut() =>").Indent()
                            .Add($"new {s.Name}({parametersString});")
                            .Unindent();
                    });
            });

        return fileScope.ToString();
    }

    public static string BuildFixtureFactory(ISymbol source)
    {
        var s = (INamedTypeSymbol)source;

        var fileScope = new CodeBuilder();
        fileScope.AddFileHeader()
            .Add("#nullable enable")
            .Scope($"namespace SweetMock", namespaceScope =>
                namespaceScope
                    .Scope("internal static partial class Fixture", classScope =>
                        classScope
                            .Documentation(d => d
                                .Summary($"Represents a test fixture designed for the {s.ToSeeCRef()} class, leveraging mocked dependencies for unit testing.")
                                .Parameter("config","An optional configuration action to customize the mocked dependencies or fixture setup.")
                                .Returns($"Returns a fixture object configured for testing the {s.ToSeeCRef()} class.")
                            )
                            .Scope($"public static {s.ContainingNamespace}.FixtureFor_{s.Name} {s.Name}(System.Action<{s.ContainingNamespace}.FixtureFor_{s.Name}.FixtureConfig>? config = null)", methodScope =>
                            methodScope
                                .Add($"var result = new {s.ContainingNamespace}.FixtureFor_{s.Name}(config);")
                                .Add("return result;"))));
        return fileScope.ToString();
    }
}
