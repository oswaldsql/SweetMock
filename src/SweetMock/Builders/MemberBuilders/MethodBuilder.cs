namespace SweetMock.Builders.MemberBuilders;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Utils;

/// <summary>
///     Represents a builder for methods, implementing the ISymbolBuilder interface.
/// </summary>
internal class MethodBuilder : IBaseClassBuilder, ILoggingExtensionBuilder
{
    public bool TryBuildBase(MockDetails details, CodeBuilder result, ISymbol[] symbols)
    {
        var first = symbols.First();

        if (first is not IMethodSymbol { MethodKind: MethodKind.Ordinary }) return false;

        var methodSymbols = symbols.OfType<IMethodSymbol>().Where(t => t.MethodKind == MethodKind.Ordinary);
        return BuildMethods(result, methodSymbols, details);
    }

    public bool TryBuildLoggingExtension(MockDetails details, CodeBuilder result, ISymbol[] symbols)
    {
        return false;
    }

    /// <summary>
    ///     Builds methods from the provided method symbols and adds them to the code builder.
    /// </summary>
    /// <param name="mockCode">The code builder to add the methods to.</param>
    /// <param name="methodSymbols">The method symbols to build methods from.</param>
    /// <returns>True if at least one method was built; otherwise, false.</returns>
    private static bool BuildMethods(CodeBuilder mockCode, IEnumerable<IMethodSymbol> methodSymbols, MockDetails target)
    {
        var enumerable = methodSymbols as IMethodSymbol[] ?? methodSymbols.ToArray();

        var name = enumerable.First().Name;

        mockCode.Add($"#region Method : {name}").Add().Indent();

        var methodCount = 1;
        foreach (var symbol in enumerable)
            if (Build(mockCode, symbol, methodCount))
                methodCount++;

        mockCode.Add().Unindent().Add("#endregion");

        return methodCount > 1;
    }

    /// <summary>
    ///     Builds a method and adds it to the code builder.
    /// </summary>
    /// <param name="builder">The code builder to add the method to.</param>
    /// <param name="symbol">The method symbol to build the method from.</param>
    /// <param name="helpers">A list of helper methods to be added.</param>
    /// <param name="methodCount">The count of methods built so far.</param>
    /// <returns>True if the method was built; otherwise, false.</returns>
    private static bool Build(CodeBuilder builder, IMethodSymbol symbol, int methodCount)
    {
        if (!(symbol.IsAbstract || symbol.IsVirtual))
        {
            builder.Add().Add("// Ignoring " + symbol);
            return false;
        }

//        if (symbol.ReturnsByRef || symbol.ReturnsByRefReadonly)
//        {
//            throw new RefReturnTypeNotSupportedException(symbol, symbol.ContainingType);
//        }

        if (symbol.IsStatic)
        {
//            if (symbol.IsAbstract)
//            {
//                throw new StaticAbstractMembersNotSupportedException(symbol.Name, symbol.ContainingType);
//            }

            builder.Add($"// Ignoring Static method {symbol}.");
            return false;
        }

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

                      internal partial class Config{
                          /// <summary>
                          /// Delegate for calling {{symbol}}
                          /// </summary>
                          public delegate {{delegateInfo.Type}} {{delegateInfo.Name}}({{delegateInfo.Parameters}});
                      
                          /// <summary>
                          /// Configures the mock to execute the specified action when the method matching the signature is called.
                          /// </summary>
                          /// <param name="call">The action or function to execute when the method is called.</param>
                          /// <returns>The mock Configuration</returns>
                          public Config {{method.Name}}({{delegateInfo.Name}} call){
                              target.{{functionPointer}} = call;
                              return this;
                          }
                      }
                      """);

        //helpers.AddRange(AddConfigExtensions(symbol, method, delegateInfo, parameters));

        return true;
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

    /// <summary>
    ///     Adds helper methods for the given method symbol.
    /// </summary>
    /// <param name="symbol">The method symbol to add helpers for.</param>
    /// <param name="methodName">The function pointer for the method.</param>
    /// <param name="parameterList">The list of parameters for the method.</param>
    /// <param name="delegateType">The delegate type for the method.</param>
    /// <param name="typeList">The list of types for the method parameters.</param>
    /// <param name="nameList">The list of names for the method parameters.</param>
    /// <param name="delegateName">Name of the delegate</param>
    /// <returns>An enumerable of helper methods.</returns>
    //    private static IEnumerable<ConfigExtension> AddConfigExtensions(IMethodSymbol symbol, MethodInfo method, DelegateInfo delegateInfo, ParameterStrings parameters)
//    {
//        ConfigExtension cx(string signature, string code, string documentation, [CallerLineNumber] int ln = 0) => new(signature, code + "// line : " + ln, documentation,  symbol.ToString());
//
//        yield return cx("System.Exception throws", $"this.{method.Name}(call:({delegateInfo.FullName})(({delegateInfo.Parameters}) => throw throws));", Documentation.ThrowsException);
//
//        if (symbol.ReturnsVoid)
//        {
//            if (symbol.Parameters.Length == 0)
//            {
//                yield return cx("", $"this.{method.Name}(call:({delegateInfo.FullName})(() => {{}}));", Documentation.AcceptAny);
//            }
//            else if (!symbol.HasOutOrRef())
//            {
//                yield return cx("", $"this.{method.Name}(call:(({delegateInfo.FullName})(({delegateInfo.Parameters}) => {{}})));", Documentation.AcceptAny);
//            }
//        }
//
//        if (!symbol.HasOutOrRef() && !symbol.ReturnsVoid)
//        {
//            yield return cx($"{delegateInfo.Type} returns", $"this.{method.Name}(call:({delegateInfo.FullName})(({delegateInfo.Parameters}) => returns));", Documentation.SpecificValue);
//
//            var code = $$"""
//                         var {{delegateInfo.Name}}_Values = returnValues.GetEnumerator();
//                         this.{{method.Name}}(call:({{delegateInfo.Container}}{{delegateInfo.Name}})(({{delegateInfo.Parameters}}) =>
//                         {
//                             if ({{delegateInfo.Name}}_Values.MoveNext())
//                             {
//                                 return {{delegateInfo.Name}}_Values.Current;
//                             }
//
//                             {{symbol.BuildNotMockedException()}}
//                             }));
//                         """;
//            yield return cx($"System.Collections.Generic.IEnumerable<{delegateInfo.Type}> returnValues", code, Documentation.SpecificValueList);
//        }
//
//        if (symbol.IsReturningTask())
//        {
//            if (symbol.HasParameters())
//            {
//                yield return cx($"System.Action<{parameters.typeList}> call", $$"""this.{{method.Name}}(call:({{delegateInfo.FullName}})(({{delegateInfo.Parameters}}) => {call({{parameters.nameList}});return System.Threading.Tasks.Task.CompletedTask;}));""", Documentation.CallBack);
//            }
//            else
//            {
//                yield return cx("System.Action call", $$"""this.{{method.Name}}(call:({{delegateInfo.FullName}})(({{delegateInfo.Parameters}}) => {call({{parameters.nameList}});return System.Threading.Tasks.Task.CompletedTask;}));""", Documentation.CallBack);
//            }
//
//            yield return cx("", $$"""this.{{method.Name}}(call:({{delegateInfo.FullName}})(({{delegateInfo.Parameters}}) => {return System.Threading.Tasks.Task.CompletedTask;}));""", Documentation.AcceptAny);
//        }
//
//        if (symbol.IsReturningGenericTask())
//        {
//            var genericType = ((INamedTypeSymbol)symbol.ReturnType).TypeArguments.First();
//            yield return cx($"{genericType} returns", $"this.{method.Name}(call:({delegateInfo.FullName})(({delegateInfo.Parameters}) => System.Threading.Tasks.Task.FromResult(returns)));", Documentation.GenericTaskObject);
//
//            var code = $$"""
//                         var {{delegateInfo.Name}}_Values = returnValues.GetEnumerator();
//                         this.{{method.Name}}(call:({{delegateInfo.FullName}})(({{delegateInfo.Parameters}}) =>
//                         {
//                             if ({{delegateInfo.Name}}_Values.MoveNext())
//                             {
//                                 return System.Threading.Tasks.Task.FromResult({{delegateInfo.Name}}_Values.Current);
//                             }
//
//                             {{symbol.BuildNotMockedException()}}
//                             }));
//                         """;
//            yield return cx($"System.Collections.Generic.IEnumerable<{genericType}> returnValues", code, Documentation.SpecificValueList);
//
//            if (symbol.HasParameters())
//            {
//                yield return cx($"System.Func<{parameters.typeList},{genericType}> callAsTask", $"this.{method.Name}(call:({delegateInfo.FullName})(({delegateInfo.Parameters}) => System.Threading.Tasks.Task.FromResult(callAsTask({parameters.nameList}))));", Documentation.GenericTaskFunction);
//            }
//            else
//            {
//                yield return cx($"System.Func<{genericType}> callAsTask", $"this.{method.Name}(call:({delegateInfo.FullName})(({delegateInfo.Parameters}) => System.Threading.Tasks.Task.FromResult(callAsTask({parameters.nameList}))));", Documentation.GenericTaskFunction);
//            }
//        }
//
//        if (symbol.IsReturningValueTask())
//        {
//            if (symbol.HasParameters())
//            {
//                yield return cx($"System.Action<{parameters.typeList}> callAsTask", $$"""this.{{method.Name}}(call:({{delegateInfo.FullName}})(({{delegateInfo.Parameters}}) => {callAsTask({{parameters.nameList}});return System.Threading.Tasks.ValueTask.CompletedTask;}));""", Documentation.CallBack);
//            }
//            else
//            {
//                yield return cx("System.Action callAsTask", $$"""this.{{method.Name}}(call:({{delegateInfo.FullName}})(({{delegateInfo.Parameters}}) => {callAsTask({{parameters.nameList}});return System.Threading.Tasks.ValueTask.CompletedTask;}));""", Documentation.CallBack);
//            }
//
//            yield return cx("", $$"""this.{{method.Name}}(call:({{delegateInfo.FullName}})(({{delegateInfo.Parameters}}) => {return System.Threading.Tasks.ValueTask.CompletedTask;}));""", Documentation.AcceptAny);
//        }
//
//        if (symbol.IsReturningGenericValueTask())
//        {
//            var genericType = ((INamedTypeSymbol)symbol.ReturnType).TypeArguments.First();
//            yield return cx($"{genericType} returns", $"this.{method.Name}(call:({delegateInfo.FullName})(({delegateInfo.Parameters}) => System.Threading.Tasks.ValueTask.FromResult(returns)));", Documentation.GenericTaskObject);
//
//            var code = $$"""
//                         var {{delegateInfo.Name}}_Values = returnValues.GetEnumerator();
//                         this.{{method.Name}}(call:({{delegateInfo.FullName}})(({{delegateInfo.Parameters}}) =>
//                         {
//                             if ({{delegateInfo.Name}}_Values.MoveNext())
//                             {
//                                 return System.Threading.Tasks.ValueTask.FromResult({{delegateInfo.Name}}_Values.Current);
//                             }
//
//                             {{symbol.BuildNotMockedException()}}
//                             }));
//                         """;
//            yield return cx($"System.Collections.Generic.IEnumerable<{genericType}> returnValues", code, Documentation.SpecificValueList);
//
//            if (symbol.HasParameters())
//            {
//                yield return cx($"System.Func<{parameters.typeList},{genericType}> callAsTask", $"this.{method.Name}(call:({delegateInfo.FullName})(({delegateInfo.Parameters}) => System.Threading.Tasks.ValueTask.FromResult(callAsTask({parameters.nameList}))));", Documentation.GenericTaskFunction);
//            }
//            else
//            {
//                yield return cx($"System.Func<{genericType}> callAsTask", $"this.{method.Name}(call:({delegateInfo.FullName})(({delegateInfo.Parameters}) => System.Threading.Tasks.ValueTask.FromResult(callAsTask({parameters.nameList}))));", Documentation.GenericTaskFunction);
//            }
//        }
//    }
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