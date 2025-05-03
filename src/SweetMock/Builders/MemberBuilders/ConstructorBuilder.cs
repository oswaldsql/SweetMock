namespace SweetMock.Builders.MemberBuilders;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
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

    private static void BuildConstructors(CodeBuilder classScope, MockDetails details, IEnumerable<IMethodSymbol> constructors)
    {
        using (classScope.Region("Constructors"))
        {
            foreach (var constructor in constructors)
            {
                var parameterList = constructor.Parameters.ToString(p => $"{p.Type} {p.Name}, ", "");
                var baseArguments = constructor.Parameters.ToString(p => p.Name);
                var argumentList = constructor.Parameters.ToString(p => $"{p.Name}, ", "");

                var constructorSignature = $"internal protected {details.MockName}({parameterList}System.Action<Config>? config = null) : base({baseArguments})";

                classScope.Scope(constructorSignature, b => b
                    .Add("var result = new Config(this, config);")
                    .BuildLogSegment(constructor)
                );

                using (classScope.AddToConfig())
                {
                    classScope.AddSummary($"Creates a new instance of <see cref=\"{details.Target.ToCRef()}\"/>");
                    classScope.Add($"public static {details.SourceName} CreateNewMock({parameterList}System.Action<Config>? config = null) => new {details.MockType}({argumentList}config);");
                }
            }
        }
    }

    private static void BuildEmptyConstructor(CodeBuilder classScope, MockDetails details)
    {
        using (classScope.Region("Constructors"))
        {
            classScope.AddLines($$"""
                             internal protected MockOf_{{details.Target.Name}}(System.Action<Config>? config = null) {
                                 var result = new Config(this, config);
                                 if(_hasLog) {
                                    _log.Add("{{details.Target}}.{{details.Target.Name}}()");
                                 }
                             }
                             """);
            using (classScope.AddToConfig())
            {
                classScope.AddSummary($"Creates a new instance of <see cref=\"{details.Target.ToCRef()}\"/>");
                classScope.Add($"public static {details.SourceName} CreateNewMock(System.Action<Config>? config = null) => new {details.MockType}(config);");
            }
        }
    }
}
