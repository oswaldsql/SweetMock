namespace SweetMock.Builders.MemberBuilders;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using SweetMock.Utils;

/// <summary>
/// Represents a builder for constructing mock constructors.
/// </summary>
internal class ConstructorBuilder : IBaseClassBuilder
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
    
    private void Build(MockDetails details, CodeBuilder result, IEnumerable<IMethodSymbol> constructors)
    {
        using (result.Region("Constructors"))
        {
            foreach (var constructor in constructors)
            {
                var parameterList = constructor.Parameters.ToString(p => $"{p.Type} {p.Name}, ", "");
                var argumentList = constructor.Parameters.ToString(p => p.Name);
                var argumentList2 = constructor.Parameters.ToString(p => $"{p.Name}, ", "");
                

                result.Add($$"""
                             internal protected {{details.MockName}}({{parameterList}}System.Action<Config>? config = null) : base({{argumentList}}) {
                                 var result = new Config(this);
                                 config?.Invoke(result);
                                 _config = result;
                                 
                                 {{LogBuilder.BuildLogSegment(constructor)}}
                             }
                             
                             internal partial class Config{
                                 /// <summary>
                                 ///     Creates a new instance of <see cref="{{details.Target.ToCRef()}}"/>
                                 /// </summary>
                                 public static {{details.SourceName}} CreateNewMock({{parameterList}}System.Action<Config>? config = null) => new {{details.MockType}}({{argumentList2}}config);
                             }
                             """);
            }
        }
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
