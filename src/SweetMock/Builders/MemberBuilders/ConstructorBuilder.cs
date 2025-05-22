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
            foreach (var constructor in constructors)
            {
                var parameterList = constructor.Parameters.ToString(p => $"{p.Type} {p.Name}, ", "");
                var baseArguments = constructor.Parameters.ToString(p => p.Name);

                var constructorSignature = $"internal protected {details.MockName}({parameterList}System.Action<Config>? config = null, SweetMock.MockOptions? options = null) : base({baseArguments})";

                builder.Scope(constructorSignature, ctor => ctor
                    .Add("_log = options?.Logger;")
                    //.Add("_hasLog = options?.Logger != null;")
                    .Add("var result = new Config(this, config);")
                    .BuildLogSegment(constructor)
                );
            }
        });

    private static void BuildEmptyConstructor(CodeBuilder classScope, MockDetails details) =>
        classScope.Region("Constructors", builder =>
        {
            builder.AddLines($$"""
                               internal protected MockOf_{{details.Target.Name}}(System.Action<Config>? config = null, SweetMock.MockOptions? options = null) {
                                   _log = options?.Logger;
                                   //_hasLog = options?.Logger != null;
                                   var result = new Config(this, config);
                                   if(_log != null) {
                                      _log.Add("{{details.Target}}.{{details.Target.Name}}()");
                                   }
                               }
                               """);
        });
}
