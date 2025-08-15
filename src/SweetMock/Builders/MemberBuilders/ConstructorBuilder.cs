namespace SweetMock.Builders.MemberBuilders;

using Generation;
using Utils;

/// <summary>
/// Represents a builder for constructing mock constructors.
/// </summary>
internal static class ConstructorBuilder {
    public static void Build(CodeBuilder classScope, MockDetails details, IEnumerable<IMethodSymbol> constructors)
    {
        var distinctConstructors = constructors.Distinct(SymbolEqualityComparer.Default).OfType<IMethodSymbol>().ToArray();

        if (distinctConstructors.Length != 0)
        {
            BuildConstructors(classScope, distinctConstructors, details.Target);
        }
        else
        {
            BuildEmptyConstructor(classScope, details.Target);
        }
    }

    private static void BuildConstructors(CodeBuilder classScope, IEnumerable<IMethodSymbol> constructors, INamedTypeSymbol target) =>
        classScope.Region("Constructors", builder =>
        {
            builder.Add("SweetMock.MockOptions? _sweetMockOptions {get;set;}");
            builder.Add("string _sweetMockInstanceName {get; set;} = \"\";");

            foreach (var constructor in constructors)
            {
                var parameterList = constructor.Parameters.ToString(p => $"{p.Type} {p.Name}, ", "");
                var baseArguments = constructor.Parameters.ToString(p => p.Name);

                var constructorSignature = $"internal protected MockOf_{target.Name}({parameterList}System.Action<Config>? config = null, SweetMock.MockOptions? options = null) : base({baseArguments})";

                builder.Scope(constructorSignature, ctor => ctor
                    .Add("_sweetMockOptions = options ?? SweetMock.MockOptions.Default;")
                    .Add("_sweetMockCallLog = _sweetMockOptions.Logger;")
                    .Add($"_sweetMockInstanceName = _sweetMockOptions.InstanceName ?? \"{target.Name}\";")
                    .Add("new Config(this, config);")
                    .BuildLogSegment(constructor)
                );
            }
        });

    private static void BuildEmptyConstructor(CodeBuilder classScope, INamedTypeSymbol target) =>
        classScope.Region("Constructors", builder => builder
            .Add("SweetMock.MockOptions? _sweetMockOptions {get;set;}")
            .Add("string _sweetMockInstanceName {get; set;} = \"\";")
            .Scope($"internal protected MockOf_{target.Name}(System.Action<Config>? config = null, SweetMock.MockOptions? options = null)", methodScope => methodScope
                .Add("_sweetMockOptions = options ?? SweetMock.MockOptions.Default;")
                .Add("_sweetMockCallLog = options?.Logger;")
                .Add($"_sweetMockInstanceName = _sweetMockOptions.InstanceName ?? \"{target.Name}\";")
                .Scope("if(_sweetMockCallLog != null)", b2 => b2
                    .Add($"_sweetMockCallLog.Add(\"{target}.{target.Name}()\");"))
                .Add("new Config(this, config);")));
}
