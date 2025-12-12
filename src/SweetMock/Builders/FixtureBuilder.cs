namespace SweetMock.Builders;

using System.Collections.Frozen;
using Generation;
using Utils;

public partial class FixtureBuilder(FixtureBuilder.FixtureMetadata metadata, List<MockInfo> mockInfos)
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

    public static string BuildFixture(FixtureMetadata metadata, List<MockInfo> mockInfos) =>
        new FixtureBuilder(metadata, mockInfos).BuildFixture();

    private readonly FrozenDictionary<INamedTypeSymbol, MockInfo> mocks = mockInfos.ToFrozenDictionary(t => t.Source, NamedSymbolEqualityComparer.Default);

    private string BuildFixture()
    {
        var fileScope = new CodeBuilder();

        fileScope.AddFileHeader()
            .Nullable()
            .Add($"namespace {metadata.Namespace};")
            .AddGeneratedCodeAttrib()
            .Scope($"internal class FixtureFor_{metadata.Name}{metadata.Generics}{metadata.Constraints}", classScope =>
            {
                this.AddFixtureConfigObject(classScope);
                this.AddPrivateMockObjects(classScope);
                this.AddCallLog(classScope);
                this.AddConstructor(classScope);
                this.AddCreateSutMethod(classScope);
            });

        return fileScope.ToString();
    }

    private void AddFixtureConfigObject(CodeBuilder builder) =>
        builder
            .Scope("internal class FixtureConfig", configScope =>
            {
                var configParameters = string.Join(", ", this.BuildFixtureConfigParameters());
                configScope.Documentation(doc =>
                    {
                        doc.Summary("Configuration object for the fixture");
                        doc.Parameters(metadata.Parameters, p => $"Configuring the {p.Name} ({p.Type.ToSeeCRef()}) mock for the fixture {metadata.ToSeeCRef}.");
                    })
                    .Scope($"internal FixtureConfig({configParameters})", ctorScope =>
                        ctorScope
                            .AddMultiple(metadata.Parameters, parameter => $"this.{parameter.Name} = {parameter.Name};")
                            .Add("this.CallLog = callLog;")
                    );

                foreach (var parameter in metadata.Parameters)
                {
                    var type = (INamedTypeSymbol)parameter.Type;
                    var generics = type.GetTypeGenerics();
                    if (this.mocks.TryGetValue(type.OriginalDefinition, out var mockInfo))
                    {
                        configScope
                            .BR()
                            .Documentation($"Gets the configuration for {parameter.Name} used within the fixture.")
                            .Add($"internal global::{mockInfo.MockClass}{generics}.{mockInfo.ContextConfigName} {parameter.Name} {{get;private set;}}");
                    }
                    else
                    {
                        configScope
                            .BR()
                            .Documentation($"Gets or sets the {parameter.Name} used for configuration within the fixture.")
                            .Add($"internal {parameter.Type.ToDisplayString(Format.ToFullNameFormat2)}{generics}? {parameter.Name} {{get; set;}}");
                    }
                }

                configScope.BR().Add("internal global::SweetMock.CallLog CallLog {get; private set;}");
            })
            .BR()
            .Documentation("Gets or sets the configuration object for the fixture used in the test setup process.", "This property enabled configuration and management of the mocked dependencies.")
            .Add("internal FixtureConfig Config{get; private set;}")
            .BR();

    private void AddPrivateMockObjects(CodeBuilder builder)
    {
        foreach (var parameter in metadata.Parameters)
        {
            var type = parameter.Type as INamedTypeSymbol;
            if (this.mocks.TryGetValue(type!.OriginalDefinition, out var mockInfo))
            {
                var generics = type.GetTypeGenerics();
                builder.Add($"private readonly {mockInfo.Source.ToDisplayString(Format.ToFullNameFormatWithoutGeneric)}{generics} _{parameter.Name};");
            }
        }

        builder.BR();
    }

    private void AddCallLog(CodeBuilder builder) =>
        builder
            .Documentation("Gets the call log used to record method invocations and interactions within the mocked dependencies during the test execution process.", "This property facilitates the tracking and validation of method calls made on the mocks in the scope of the unit tests.")
            .Add("private global::SweetMock.CallLog _log;")
            .Add("public global::SweetMock.CallLog Log {get; private set;}")
            .Add("public FixtureLogger Calls {get; private set;}")
            .Scope("internal class FixtureLogger(global::SweetMock.CallLog callLog) : global::SweetMock.FixtureLog_Base(callLog)", classScope =>
            {
                foreach (var parameter in metadata.Parameters)
                {
                    var type = parameter.Type as INamedTypeSymbol;
                    if (this.mocks.TryGetValue(type!.OriginalDefinition, out var parameterInfo))
                    {
                        var generics = type.GetTypeGenerics();
                        classScope
                            .Add($"public global::{parameterInfo.MockClass}{generics}.{parameterInfo.Source.Name}_Logs {parameter.Name} = new(callLog, \"{parameter.Name}\");")
                            .BR();
                    }
                }
            })
            .BR();

    private void AddConstructor(CodeBuilder builder) =>
        builder
            .Documentation(doc => doc
                .Summary($"Provides a fixture for the {metadata.ToSeeCRef} object, setting up mocks and a call log for testing purposes.")
                .Parameter("config", "Optional configuration of the mocked dependencies.")
            )
            .Scope($"public FixtureFor_{metadata.Name}(System.Action<FixtureConfig>? config = null)", ctorScope =>
            {
                ctorScope
                    .Add("_log = new SweetMock.CallLog();")
                    .Add("Log = _log;")
                    .Add("Calls = new FixtureLogger(_log);")
                    .BR();

                foreach (var parameter in metadata.Parameters)
                {
                    var type = parameter.Type as INamedTypeSymbol;
                    if (this.mocks.ContainsKey(type!.OriginalDefinition))
                    {
                        var generics = type.GetTypeGenerics();
                        ctorScope.Add($"_{parameter.Name} = global::SweetMock.Mock.{type.Name}{generics}(out var temp_{parameter.Name}, new(_log, \"{parameter.Name}\"));");
                    }
                    else
                    {
                        ctorScope.Add($"{type.ToDisplayString(Format.ToFullNameFormat2)}? temp_{parameter.Name} = default;").BR();
                    }
                }

                var parametersString = metadata.Parameters.ToString(t => "temp_" + t.Name);
                parametersString += parametersString == "" ? "_log" : ", _log";

                builder
                    .BR()
                    .Add($"Config = new FixtureConfig({parametersString});")
                    .Add("config?.Invoke(Config);");
            }).BR();

    private void AddCreateSutMethod(CodeBuilder builder)
    {
        var arguments = metadata.Parameters.ToString(parameter => $"{parameter.Type.AsNullable()} {parameter.Name} = null");

        builder
            .Documentation(doc => doc
                .Summary($"Creates an instance of the {metadata.ToSeeCRef} object using the initialized mock dependencies.")
                .Parameters(metadata.Parameters, symbol => $"Explicitly sets the value for {symbol.Name} bypassing the values created by the fixture.")
                .Returns($"A {metadata.ToSeeCRef} instance configured with mocked dependencies.")
            )
            .Scope($"public {metadata.TypeString} Create{metadata.Name}({arguments})", methodScope =>
            {
                methodScope
                    .AddMultiple(metadata.Parameters, parameter => $"var argument_{parameter.Name} = {parameter.Name} ?? {this.MockTypeToArgument(parameter)};")
                    .Add($"return new {metadata.TypeString}({metadata.Parameters.ToString(symbol => "argument_" + symbol.Name)});");
            });
    }

    private string MockTypeToArgument(IParameterSymbol parameter)
    {
        var canBeNull = parameter.Type.NullableAnnotation == NullableAnnotation.Annotated;
        var mockType = this.GetMockType(parameter);
        return mockType switch
        {
            MockKind.Wrapper => $"_{parameter.Name}.Value",
            MockKind.BuildIn => $"_{parameter.Name}",
            MockKind.Generated => $"_{parameter.Name}",
            MockKind.Direct when canBeNull => $"Config.{parameter.Name}",
            MockKind.Direct when !canBeNull => $"Config.{parameter.Name} ?? throw new NullReferenceException()",
            _ => ""
        };
    }

    private MockKind GetMockType(IParameterSymbol parameter) =>
        this.mocks.TryGetValue((INamedTypeSymbol)parameter.Type.OriginalDefinition, out var result)
            ? result.Kind
            : MockKind.Direct;

    private IEnumerable<string> BuildFixtureConfigParameters()
    {
        foreach (var parameter in metadata.Parameters)
        {
            var type = (INamedTypeSymbol)parameter.Type;
            var generics = type.GetTypeGenerics();
            if (this.mocks.TryGetValue(type.OriginalDefinition, out var info))
            {
                yield return $"global::{info.MockClass}{generics}.{info.ContextConfigName} {parameter.Name}";
            }
            else
            {
                yield return $"{parameter.Type.ToDisplayString(Format.ToFullNameFormat2)}? {parameter.Name}";
            }
        }

        yield return "global::SweetMock.CallLog callLog";
    }
}
