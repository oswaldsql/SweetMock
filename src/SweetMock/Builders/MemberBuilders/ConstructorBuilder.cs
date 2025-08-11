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

        if (distinctConstructors.Any())
        {
            BuildConstructors(classScope, details, distinctConstructors);
        }
        else
        {
            BuildEmptyConstructor(classScope, details);
        }
    }

    private static void BuildConstructors(CodeBuilder classScope, MockDetails details, IEnumerable<IMethodSymbol> constructors) =>
        classScope.Region("Constructors", builder =>
        {
            builder.Add("SweetMock.MockOptions? _sweetMockOptions {get;set;}");

            foreach (var constructor in constructors)
            {
                var parameterList = constructor.Parameters.ToString(p => $"{p.Type} {p.Name}, ", "");
                var baseArguments = constructor.Parameters.ToString(p => p.Name);

                var constructorSignature = $"internal protected {details.MockName}({parameterList}System.Action<Config>? config = null, SweetMock.MockOptions? options = null) : base({baseArguments})";

                builder.Scope(constructorSignature, ctor => ctor
                    .Add("_sweetMockCallLog = options?.Logger;")
                    .Add("_sweetMockOptions = options ?? SweetMock.MockOptions.Default;")
                    .Add("new Config(this, config);")
                    .BuildLogSegment(constructor)
                );
            }
        });

    private static void BuildEmptyConstructor(CodeBuilder classScope, MockDetails details) =>
        classScope.Region("Constructors", builder => builder
            .Add("SweetMock.MockOptions? _sweetMockOptions {get;set;}")
            .Scope($"internal protected MockOf_{details.Target.Name}(System.Action<Config>? config = null, SweetMock.MockOptions? options = null)", methodScope => methodScope
                .Add("_sweetMockCallLog = options?.Logger;")
                .Add("_sweetMockOptions = options ?? SweetMock.MockOptions.Default;")
                .Scope("if(_sweetMockCallLog != null)", b2 => b2
                    .Add($"_sweetMockCallLog.Add(\"{details.Target}.{details.Target.Name}()\");"))
                .Add("new Config(this, config);")));
}
