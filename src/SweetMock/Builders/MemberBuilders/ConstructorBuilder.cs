namespace SweetMock.Builders.MemberBuilders;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using SweetMock.Utils;

/// <summary>
/// Represents a builder for constructing mock constructors.
/// </summary>
internal static class ConstructorBuilder {
    public static CodeBuilder Build(MockDetails details, IEnumerable<IMethodSymbol> constructors)
    {
        constructors = constructors.Distinct().ToArray();

        return !constructors.Any() ? BuildEmptyConstructor(details) : BuildConstructors(details, constructors);
    }

    private static CodeBuilder BuildConstructors(MockDetails details, IEnumerable<IMethodSymbol> constructors)
    {
        CodeBuilder result = new();

        using (result.Region("Constructors"))
        {
            foreach (var constructor in constructors)
            {
                var parameterList = constructor.Parameters.ToString(p => $"{p.Type} {p.Name}, ", "");
                var baseArguments = constructor.Parameters.ToString(p => p.Name);
                var argumentList = constructor.Parameters.ToString(p => $"{p.Name}, ", "");

                result.Add($$"""
                             internal protected {{details.MockName}}({{parameterList}}System.Action<Config>? config = null) : base({{baseArguments}}) {
                                 var result = new Config(this);
                                 config?.Invoke(result);
                                 _config = result;

                                 {{LogBuilder.BuildLogSegment(constructor)}}
                             }

                             internal partial class Config{
                                 /// <summary>
                                 ///     Creates a new instance of <see cref="{{details.Target.ToCRef()}}"/>
                                 /// </summary>
                                 public static {{details.SourceName}} CreateNewMock({{parameterList}}System.Action<Config>? config = null) => new {{details.MockType}}({{argumentList}}config);
                             }
                             """);
            }
        }

        return result;
    }

    private static CodeBuilder BuildEmptyConstructor(MockDetails details)
    {
        CodeBuilder result = new();

        using (result.Region("Constructors"))
        {
            result.Add($$"""
                         internal protected MockOf_{{details.Target.Name}}(System.Action<Config>? config = null) {
                             var result = new Config(this);
                             config?.Invoke(result);
                             _config = result;
                             if(_hasLog) {
                                _log.Add("{{details.Target}}.{{details.Target.Name}}()");
                             }
                         }

                         internal partial class Config{
                             public static {{details.SourceName}} CreateNewMock(System.Action<Config>? config = null) => new {{details.MockType}}(config);
                         }
                         """);
        }

        return result;
    }
}
