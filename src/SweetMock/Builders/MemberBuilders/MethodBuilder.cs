namespace SweetMock.Builders.MemberBuilders;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Utils;

/// <summary>
///     Represents a builder for methods, implementing the ISymbolBuilder interface.
/// </summary>
internal static class MethodBuilder
{
    public static CodeBuilder Build(IEnumerable<IMethodSymbol> methods)
    {
        using CodeBuilder result = new();

        var lookup = methods.ToLookup(t => t.Name);
        foreach (var m in lookup)
        {
            result.Add(BuildMethods(m.ToArray()));
        }

        return result;
    }

    /// <summary>
    ///     Builds methods from the provided method symbols and adds them to the code builder.
    /// </summary>
    /// <param name="mockCode">The code builder to add the methods to.</param>
    /// <param name="methodSymbols">The method symbols to build methods from.</param>
    /// <returns>True if at least one method was built; otherwise, false.</returns>
    private static CodeBuilder BuildMethods(IMethodSymbol[] methodSymbols)
    {
        using CodeBuilder result = new();

        var name = methodSymbols.First().Name;

        using (result.Region($"Method : {name}"))
        {
            var methodCount = 1;
            foreach (var symbol in methodSymbols)
            {
                result.Add(Build(symbol, methodCount));
                methodCount++;
            }
        }

        return result;
    }

    /// <summary>
    ///     Builds a method and adds it to the code builder.
    /// </summary>
    /// <param name="builder">The code builder to add the method to.</param>
    /// <param name="symbol">The method symbol to build the method from.</param>
    /// <param name="methodCount">The count of methods built so far.</param>
    /// <returns>True if the method was built; otherwise, false.</returns>
    private static CodeBuilder Build(IMethodSymbol symbol, int methodCount)
    {
        if(symbol.ReturnsByRef) throw new Exception("Property has returns byref");

        using CodeBuilder builder = new();

        var parameters = symbol.ParameterStrings();

        var method = MethodMetadata(symbol);

        var overwrites = symbol.Overwrites();

        var delegateInfo = DelegateInfo(symbol, methodCount);

        var functionPointer = methodCount == 1 ? $"_{method.Name}" : $"_{method.Name}_{methodCount}";

        var genericString = GenericString(symbol);
        var castString = symbol is { IsGenericMethod: true, ReturnsVoid: false } ? " (" + method.ReturnType + ") " : "";

        builder.Add($$"""
                      {{overwrites.accessibilityString}}{{overwrites.overrideString}}{{method.ReturnType}} {{overwrites.containingSymbol}}{{method.Name}}{{genericString}}({{parameters.methodParameters}})
                      {
                          {{LogBuilder.BuildLogSegment(symbol)}}
                          {{method.ReturnString}}{{castString}}this.{{functionPointer}}.Invoke({{parameters.nameList}});
                      }
                      private Config.{{delegateInfo.Name}} {{functionPointer}} {get;set;} = ({{delegateInfo.Parameters}}) => {{symbol.BuildNotMockedException()}}
                      """);

        using (builder.AddToConfig())
        {
            builder.AddSummary($"Delegate for calling <see cref=\"{symbol.ToCRef()}\"/>");
            builder.Add($"public delegate {delegateInfo.Type} {delegateInfo.Name}({delegateInfo.Parameters});");
            builder.AddSummary($"Configures the mock to execute the specified action when calling <see cref=\"{symbol.ToCRef()}\"/>.");
            builder.AddParameter("call", "The action or function to execute when the method is called.");
            builder.AddReturns("The mock Configuration");
            builder.Add($$"""
                              public Config {{method.Name}}({{delegateInfo.Name}} call){
                                  target.{{functionPointer}} = call;
                                  return this;
                              }
                          """);
        }

        return builder;
    }

    private static DelegateInfo DelegateInfo(IMethodSymbol symbol, int methodCount)
    {
        var delegateName = methodCount == 1 ? $"DelegateFor_{symbol.Name}" : $"DelegateFor_{symbol.Name}_{methodCount}";
        var delegateType = symbol is { IsGenericMethod: true, ReturnsVoid: false } ? "object" : symbol.ReturnType.ToString();
        var delegateContainer = "--MockClass--";

        var parameters = symbol.Parameters.Select(t => new ParameterInfo(t.Type.ToString(), t.Name, t.OutAsString(), t.Name)).ToList();

        if (symbol.IsGenericMethod) parameters.AddRange(symbol.TypeArguments.Select(typeArgument => new ParameterInfo("System.Type", "typeOf_" + typeArgument.Name, "", "typeof(" + typeArgument.Name + ")")));

        var parameterList = parameters.ToString(p => $"{p.OutString}{p.Type} {p.Name}");

        return new(delegateName, delegateType, delegateContainer, delegateContainer + delegateName, parameterList);
    }

    private static MethodInfo MethodMetadata(IMethodSymbol method)
    {
        var methodName = method.Name;
        var methodReturnType = method.ReturnType.ToString();
        var returnString = method.ReturnsVoid ? "" : "return ";

        return new(methodName, methodReturnType, returnString);
    }

    private static string GenericString(IMethodSymbol symbol)
    {
        if (!symbol.IsGenericMethod) return "";

        var typeArguments = symbol.TypeArguments;
        var types = string.Join(", ", typeArguments.Select(t => t.Name));
        return $"<{types}>";
    }
}

public record DelegateInfo(string Name, string Type, string Container, string FullName, string Parameters);

public record MethodInfo(string Name, string ReturnType, string ReturnString);
