namespace SweetMock.Builders.MemberBuilders;

using Exceptions;
using Generation;
using Utils;

/// <summary>
///     Represents a builder for methods, implementing the ISymbolBuilder interface.
/// </summary>
internal static class MethodBuilder
{
    public static void Build(CodeBuilder classScope, IEnumerable<IMethodSymbol> methods)
    {
        var lookup = methods.ToLookup(t => t.Name);
        foreach (var m in lookup)
        {
            BuildMethods(classScope, m.ToArray());
        }
    }

    /// <summary>
    ///     Builds methods from the provided method symbols and adds them to the code builder.
    /// </summary>
    /// <param name="classScope">The code builder for the class scope.</param>
    /// <param name="methodSymbols">The method symbols to build methods from.</param>
    /// <returns>True if at least one method was built; otherwise, false.</returns>
    private static void BuildMethods(CodeBuilder classScope, IMethodSymbol[] methodSymbols)
    {
        var name = methodSymbols.First().Name;
        classScope.Region($"Method : {name}", builder =>
        {
            var methodCount = 1;
            foreach (var symbol in methodSymbols)
            {
                Build(builder, symbol, methodCount);
                methodCount++;
            }
        });
    }

    /// <summary>
    ///     Builds a method and adds it to the code builder.
    /// </summary>
    /// <param name="classScope">The code builder for the class scope.</param>
    /// <param name="symbol">The method symbol to build the method from.</param>
    /// <param name="methodCount">The count of methods built so far.</param>
    /// <returns>True if the method was built; otherwise, false.</returns>
    private static void Build(CodeBuilder classScope, IMethodSymbol symbol, int methodCount)
    {
        if (symbol.ReturnsByRef)
        {
            throw new RefReturnTypeNotSupportedException(symbol, symbol.ContainingType);
        }

        var parameters = symbol.ParameterStrings();

        var method = MethodMetadata(symbol);

        var overwrites = symbol.Overwrites();

        var delegateInfo = GetDelegateInfo(symbol, methodCount);

        var functionPointer = methodCount == 1 ? $"_{method.Name}" : $"_{method.Name}_{methodCount}";

        var genericString = GenericString(symbol);
        var castString = symbol is { IsGenericMethod: true, ReturnsVoid: false } ? " (" + method.ReturnType + ") " : "";

        var signature = $"{overwrites.AccessibilityString}{overwrites.OverrideString}{method.ReturnType} {overwrites.ContainingSymbol}{method.Name}{genericString}({parameters.MethodParameters})";
        classScope.Scope(signature, methodScope => methodScope
            .BuildLogSegment(symbol)
            .Add($"{method.ReturnString}{castString}this.{functionPointer}.Invoke({parameters.NameList});")
        );

        classScope.Add($"private Config.{delegateInfo.Name} {functionPointer} {{get;set;}} = ({delegateInfo.Parameters}) => {symbol.BuildNotMockedException()}");

        classScope.AddToConfig(config =>
        {
            config.Documentation(doc => doc
                .Summary($"Delegate for calling <see cref=\"{symbol.ToCRef()}\"/>"));
            config.Add($"public delegate {delegateInfo.Type} {delegateInfo.Name}({delegateInfo.Parameters});");

            config.Documentation(doc => doc
                .Summary($"Configures the mock to execute the specified action when calling <see cref=\"{symbol.ToCRef()}\"/>.")
                .Parameter("call", "The action or function to execute when the method is called.")
                .Returns("The updated configuration object."));

            config.AddConfigMethod(method.Name, [$"{delegateInfo.Name} call"], builder => builder
                .Add($"target.{functionPointer} = call;"));
        });

    }
    private static DelegateInfo GetDelegateInfo(IMethodSymbol symbol, int methodCount)
    {
        var delegateName = methodCount == 1 ? $"DelegateFor_{symbol.Name}" : $"DelegateFor_{symbol.Name}_{methodCount}";
        var delegateType = symbol is { IsGenericMethod: true, ReturnsVoid: false } ? "object" : symbol.ReturnType.ToString();
        var delegateContainer = "--MockClass--";

        var parameterList = symbol.GetParameterInfos().ToString(p => $"{p.OutString}{p.Type} {p.Name}");

        return new(delegateName, delegateType, delegateContainer, delegateContainer + delegateName, parameterList);
    }

    private static IEnumerable<ParameterInfo> GetParameterInfos(this IMethodSymbol symbol)
    {
        if (!symbol.IsGenericMethod)
        {
            foreach (var t in symbol.Parameters)
            {
                yield return new(t.Type.ToString(), t.Name, t.OutAsString(), t.Name);
            }

            yield break;
        }

        foreach (var parameter in symbol.Parameters)
        {
            if (parameter.Type.TypeKind == TypeKind.TypeParameter && parameter.Type.ContainingSymbol is IMethodSymbol)
            {
                yield return new("System.Object", parameter.Name, parameter.OutAsString(), parameter.Name);
            }
            else
            {
                yield return new(parameter.Type.ToString(), parameter.Name, parameter.OutAsString(), parameter.Name);
            }
        }

        foreach (var typeArgument in symbol.TypeArguments)
        {
            yield return new("System.Type", "typeOf_" + typeArgument.Name, "", "typeof(" + typeArgument.Name + ")");
        }
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
        if (!symbol.IsGenericMethod)
        {
            return "";
        }

        var typeArguments = symbol.TypeArguments;
        var types = string.Join(", ", typeArguments.Select(t => t.Name));
        return $"<{types}>";
    }

    public static void BuildConfigExtensions(CodeBuilder builder, MockDetails mock, IEnumerable<IMethodSymbol> methods)
    {
        var source = methods.ToArray();

        builder.AddReturnsExtensions(mock, source);

        builder.AddReturnValuesExtensions(mock, source);

        builder.AddNoReturnValueExtensions(mock, source);

        builder.AddThrowExtensions(mock, source);
    }

    private static void AddReturnsExtensions(this CodeBuilder result, MockDetails mock, IMethodSymbol[] source)
    {
        var methodSymbols = source.Where(m => !m.ReturnsVoid && !m.Parameters.Any(symbol => symbol.RefKind == RefKind.Out));
        var candidates = methodSymbols.ToLookup(t => t.Name + ":" + t.ReturnType);
        foreach (var candidate in candidates)
        {
            var seeString = string.Join(", ", candidate.Select(t => $"<see cref=\"{t.ToCRef()}\"/>"));

            result.AddLineBreak();
            result.Documentation(doc => doc
                .Summary("Configures the mock to return a specific value disregarding the arguments.", $"Configures {seeString}")
                .Parameter("returns", "The value that should be returned")
                .Returns("The updated configuration object."));

            var returnType = candidate.First().ReturnType.ToString();
            if (candidate.First().ReturnType.TypeKind == TypeKind.TypeParameter && candidate.First().ReturnType.ContainingSymbol is IMethodSymbol)
            {
                returnType = "System.Object";
            }

            result.AddConfigExtension(mock, candidate.First(), [returnType + " returns"], builder =>
            {
                foreach (var m in candidate)
                {
                    var parameters = GetParameterInfos(m);

                    var parameterList = parameters.ToString(p => $"{p.OutString}{p.Type} _");

                    builder.Add($"this.{m.Name}(call: ({parameterList}) => returns);");
                }
            });
        }
    }

    private static void AddReturnValuesExtensions(this CodeBuilder result, MockDetails mock, IMethodSymbol[] source)
    {
        var methodSymbols = source.Where(m => !m.ReturnsVoid && !m.Parameters.Any(symbol => symbol.RefKind == RefKind.Out));
        var candidates = methodSymbols.ToLookup(t => t.Name + ":" + t.ReturnType);
        foreach (var candidate in candidates)
        {
            var seeString = string.Join(", ", candidate.Select(t => $"<see cref=\"{t.ToCRef()}\"/>"));

            result.AddLineBreak();
            result.Documentation(doc => doc
                .Summary("Configures the mock to return a one of the specific value disregarding the arguments.", $"Configures {seeString}")
                .Parameter("returns", "The values that should be returned")
                .Returns("The updated configuration object."));

            var returnType = candidate.First().ReturnType.ToString();
            if (candidate.First().ReturnType.TypeKind == TypeKind.TypeParameter && candidate.First().ReturnType.ContainingSymbol is IMethodSymbol)
            {
                returnType = "System.Object";
            }

            returnType = "System.Collections.Generic.IEnumerable<" + returnType + ">";

            result.AddConfigExtension(mock, candidate.First(), [returnType + " returnValues"], builder =>
            {
                var index = 0;
                foreach (var m in candidate)
                {
                    index++;
                    var index1 = index;
                    var parameters = GetParameterInfos(m);
                    var parameterList = parameters.ToString(p => $"{p.OutString}{p.Type} _");

                    builder.Add($"var {m.Name}{index1}_Values = returnValues.GetEnumerator();");
                    builder.Scope($"this.{m.Name}(call: ({parameterList}) => ", lambdaScope => lambdaScope
                            .Scope($"if({m.Name}{index1}_Values.MoveNext())", conditionScope => conditionScope
                                .Add($"return {m.Name}{index1}_Values.Current;")
                            )
                            .Add(m.BuildNotMockedException()))
                        .Add(");");
                }
            });
        }
    }

    private static void AddNoReturnValueExtensions(this CodeBuilder result, MockDetails mock, IMethodSymbol[] source)
    {
        var methodSymbols = source.Where(t => (t.ReturnsVoid || t.ReturnType.ToString() == "System.Threading.Tasks.Task" || t.ReturnType.ToString() == "System.Threading.Tasks.ValueTask") && !t.Parameters.Any(s => s.RefKind == RefKind.Out));
        var candidates = methodSymbols.ToLookup(t => t.Name);
        foreach (var candidate in candidates)
        {
            var seeString = string.Join(", ", candidate.Select(t => $"<see cref=\"{t.ToCRef()}\"/>"));

            result.AddLineBreak();
            result.Documentation(doc => doc
                .Summary("Configures the mock to accept any call to methods not returning values.", $"Configures {seeString}")
                .Returns("The updated configuration object."));

            result.AddConfigExtension(mock, candidate.First(), [], builder =>
            {
                foreach (var m in candidate)
                {
                    var parameters = GetParameterInfos(m);
                    var parameterList = parameters.ToString(p => $"{p.OutString}{p.Type} _");

                    var str = m.ReturnType.ToString() switch
                    {
                        "void" => $"this.{m.Name}(call: ({parameterList}) => {{}});",
                        "System.Threading.Tasks.Task" => $"this.{m.Name}(call: ({parameterList}) => System.Threading.Tasks.Task.CompletedTask);",
                        "System.Threading.Tasks.ValueTask" => $"this.{m.Name}(call: ({parameterList}) => System.Threading.Tasks.ValueTask.CompletedTask);",
                        _ => ""
                    };

                    builder.Add(str);
                }
            });
        }
    }

    private static void AddThrowExtensions(this CodeBuilder result, MockDetails mock, IEnumerable<IMethodSymbol> methods)
    {
        foreach (var methodGroup in methods.ToLookup(t => t.Name))
        {
            var seeString = string.Join(", ", methodGroup.Select(t => $"<see cref=\"{t.ToCRef()}\"/>"));

            result.AddLineBreak();
            result.Documentation(doc => doc
                .Summary("Configures the mock to throw the specified exception when the method is called.", $"Configures {seeString}")
                .Parameter("throws", "The exception to be thrown when the method is called.")
                .Returns("The updated configuration object."));

            result.AddConfigExtension(mock, methodGroup.First(), ["Exception throws"], builder =>
            {
                foreach (var method in methodGroup)
                {
                    var parameters = GetParameterInfos(method);

                    var parameterList = parameters.ToString(p => $"{p.OutString}{p.Type} _");

                    builder.Add($"this.{method.Name}(call: ({parameterList}) => throw throws);");
                }
            });
        }
    }

    private record DelegateInfo(string Name, string Type, string Container, string FullName, string Parameters);

    private record MethodInfo(string Name, string ReturnType, string ReturnString);
}

/*
    private static IEnumerable<HelperMethod> AddHelpers(IMethodSymbol symbol, string functionPointer, string parameterList, string delegateType, string typeList, string nameList)
    {
        var seeCref = symbol.ToString();

        if (symbol.IsReturningTask())
        {
            if (symbol.HasParameters())
            {
                yield return new HelperMethod($"System.Action<{typeList}> call", $$"""this.{{functionPointer}}(({{parameterList}}) => {call({{nameList}});return System.Threading.Tasks.Task.CompletedTask;});""", Documentation.CallBack, seeCref);
            }
            else
            {
                yield return new HelperMethod("System.Action call", $$"""this.{{functionPointer}}(({{nameList}}) => {call({{nameList}});return System.Threading.Tasks.Task.CompletedTask;});""", Documentation.CallBack, seeCref);
            }
        }

        if (symbol.IsReturningGenericTask())
        {
            var genericType = ((INamedTypeSymbol)symbol.ReturnType).TypeArguments.First();
            yield return new HelperMethod($"{genericType} returns", $"this.{functionPointer}(({parameterList}) => System.Threading.Tasks.Task.FromResult(returns));", Documentation.GenericTaskObject, seeCref);

            var code = $$"""
                         var {{functionPointer}}_Values = returnValues.GetEnumerator();
                         this.{{functionPointer}}(({{parameterList}}) =>
                         {
                             if ({{functionPointer}}_Values.MoveNext())
                             {
                                 return System.Threading.Tasks.Task.FromResult({{functionPointer}}_Values.Current);
                             }

                             {{symbol.BuildNotMockedException()}}
                             });
                         """;
            yield return new HelperMethod($"System.Collections.Generic.IEnumerable<{genericType}> returnValues", code, Documentation.SpecificValueList, seeCref);

            if (symbol.HasParameters())
            {
                yield return new HelperMethod($"System.Func<{typeList},{genericType}> call", $"this.{functionPointer}(({nameList}) => System.Threading.Tasks.Task.FromResult(call({nameList})));", Documentation.GenericTaskFunction, seeCref);
            }
            else
            {
                yield return new HelperMethod($"System.Func<{genericType}> call", $"this.{functionPointer}(({nameList}) => System.Threading.Tasks.Task.FromResult(call({nameList})));", Documentation.GenericTaskFunction, seeCref);
            }
        }
 */
