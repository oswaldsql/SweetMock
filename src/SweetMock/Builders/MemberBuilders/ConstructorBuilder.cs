namespace SweetMock.Builders.MemberBuilders;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using SweetMock.Utils;

/// <summary>
/// Represents a builder for constructing mock constructors.
/// </summary>
internal class ConstructorBuilder : IBaseClassBuilder, ILoggingExtensionBuilder
{
    public bool TryBuildBase(MockDetails details, CodeBuilder result, ISymbol[] symbols)
    {
        if (symbols.All(t => t is IMethodSymbol { MethodKind: MethodKind.Constructor }))
        {
            Build(details, result, symbols.OfType<IMethodSymbol>());
            return true;
        }

        return false;
    }
    
    public bool TryBuildLoggingExtension(MockDetails details, CodeBuilder result, ISymbol[] symbols)
    {
        if (symbols.All(t => t is IMethodSymbol { MethodKind: MethodKind.Constructor }))
        {
            result.Add($"public class {details.Target.Name}_ctorArgs : SweetMock.TypedArguments {{}}");
            
            result.Add($$"""
                         public static IEnumerable<SweetMock.TypedCallLogItem<{{details.Target.Name}}_ctorArgs>> {{details.Target.Name}}(this IEnumerable<SweetMock.CallLogItem> log, Func<{{details.Target.Name}}_ctorArgs, bool>? predicate = null)
                         {
                             return log.Where(t => t.MethodSignature == "{{symbols.First().ToString()}}")
                                     .Select(t => new SweetMock.TypedCallLogItem<{{details.Target.Name}}_ctorArgs>(t))
                                     .Where(t => predicate == null || predicate(t.TypedArguments));
                         }
                         """);
            return true;
        }

        return false;
    }

    
    private void Build(MockDetails details, CodeBuilder result, IEnumerable<IMethodSymbol> constructors)
    {
        result.Add("#region Constructors").Add().Indent();

        foreach (var constructor in constructors)
        {
            var parameterList = constructor.Parameters.ToString(p => $"{p.Type} {p.Name}, ", "");
            var argumentList = constructor.Parameters.ToString(p => p.Name);

            result.Add($$"""
                         internal protected {{details.MockName}}({{parameterList}}System.Action<Config>? config = null) : base({{argumentList}}) {
                             var result = new Config(this);
                             config?.Invoke(result);
                             _config = result;
                             
                             {{LogBuilder.BuildLogSegment(constructor)}}
                         }
                         """);
        }

        result.Add().Unindent().Add("#endregion");
    }

    public static string BuildEmptyConstructor(MockDetails details) =>
        ($$"""
           #region Constructors
           ->
           
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
           
           <-
           #endregion
           """);
}
