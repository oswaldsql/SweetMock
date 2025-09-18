namespace SweetMock.Builders.MemberBuilders;

using Exceptions;
using Generation;
using Utils;

/// <summary>
///     Represents a builder for methods, implementing the ISymbolBuilder interface.
/// </summary>
internal class MethodBuilder
{
    private readonly MockContext context;
    private readonly Dictionary<IMethodSymbol, string> methodDelegateName = new(SymbolEqualityComparer.Default);
    private readonly ILookup<string, IMethodSymbol> methodGroups;

    public MethodBuilder(IEnumerable<IMethodSymbol> methods, MockContext context)
    {
        this.context = context;
        this.methodGroups = methods.ToLookup(t => t.Name);
    }

    public static void Render(CodeBuilder classScope, MockContext context, IEnumerable<IMethodSymbol> methods)
    {
        var methodBuilder = new MethodBuilder(methods, context);
        methodBuilder.Render(classScope);
    }

    public void Render(CodeBuilder classScope)
    {
        foreach (var methodGroup in this.methodGroups)
        {
            classScope.Region($"Method : {methodGroup.Key}", builder =>
            {
                var methodCount = 1;
                foreach (var symbol in methodGroup)
                {
                    this.Build(builder, symbol, methodCount);
                    methodCount++;
                }

                classScope.AddToConfig(this.context, config => this.BuildConfigExtensions(config, methodGroup));
            });
        }
    }

    /// <summary>
    ///     Builds a method and adds it to the code builder.
    /// </summary>
    /// <param name="classScope">The code builder for the class scope.</param>
    /// <param name="symbol">The method symbol to build the method from.</param>
    /// <param name="methodCount">The count of methods built so far.</param>
    /// <returns>True if the method was built; otherwise, false.</returns>
    private void Build(CodeBuilder classScope, IMethodSymbol symbol, int methodCount)
    {
        if (symbol.ReturnsByRef)
        {
            throw new RefReturnTypeNotSupportedException(symbol, symbol.ContainingType);
        }

        var (methodParameters, nameList) = symbol.ParameterStrings();

        var (name, returnType, returnString) = MethodMetadata(symbol);

        var (containingSymbol, accessibilityString, overrideString) = symbol.Overwrites();

        var (delegateName, delegateType, delegateParameters) = GetDelegateInfo(symbol, methodCount);
        this.methodDelegateName.Add(symbol, delegateName);

        var functionPointer = methodCount == 1 ? $"_{name}" : $"_{name}_{methodCount}";

        var genericString = GenericString(symbol);
        var castString = symbol is { IsGenericMethod: true, ReturnsVoid: false } ? " (" + returnType + ") " : "";

        var signature = $"{accessibilityString}{overrideString}{returnType} {containingSymbol}{name}{genericString}({methodParameters})";
        classScope.Scope(signature, methodScope => methodScope
            .BuildLogSegment(symbol)
            .Scope($"if (this.{functionPointer} is null)", ifScope =>
            {
                ifScope.Add($"throw new SweetMock.NotExplicitlyMockedException(\"{symbol.Name}\", _sweetMockInstanceName);");
            })
            .Add($"{returnString}{castString}this.{functionPointer}.Invoke({nameList});")
        );

        classScope.Add($"private {this.context.ConfigName}.{delegateName}? {functionPointer} {{get;set;}} = null;");

        classScope.AddToConfig(this.context, config =>
        {
            config
                .Documentation($"Delegate for mocking calls to {symbol.ToSeeCRef()}.")
                .Add($"public delegate {delegateType} {delegateName}({delegateParameters});");

            config
                .Documentation(doc => doc
                    .Summary($"Configures the mock to execute the specified action when calling {symbol.ToSeeCRef()}.")
                    .Parameter("call", "The action or function to execute when the method is called.")
                    .Returns("The updated configuration object."))
                .AddConfigMethod(this.context, name, [$"{delegateName} call"], methodScope => methodScope
                    .Add($"target.{functionPointer} = call;"));
        });
    }

    private static DelegateInfo GetDelegateInfo(IMethodSymbol symbol, int methodCount)
    {
        var delegateName = methodCount == 1 ? $"DelegateFor_{symbol.Name}" : $"DelegateFor_{symbol.Name}_{methodCount}";
        var delegateType = symbol is { IsGenericMethod: true, ReturnsVoid: false } ? "object" : symbol.ReturnType.ToString();

        var parameterList = GetParameterInfos(symbol).ToString(p => $"{p.OutString}{p.Type} {p.Name}");

        return new(delegateName, delegateType, parameterList);
    }

    private static IEnumerable<ParameterInfo> GetParameterInfos(IMethodSymbol symbol)
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
            if (parameter.Type is { TypeKind: TypeKind.TypeParameter, ContainingSymbol: IMethodSymbol })
            {
                yield return new("System.Object", parameter.Name, parameter.OutAsString(), parameter.Name);
            }
            else if (((INamedTypeSymbol)parameter.Type).IsGenericType)
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

    private void BuildConfigExtensions(CodeBuilder builder, IGrouping<string, IMethodSymbol> methods)
    {
        this.AddReturnsExtensions(builder, methods);
        this.AddTaskReturnsExtensions(builder, methods);
        this.AddReturnValuesExtensions(builder, methods);
        this.AddNoReturnValueExtensions(builder, methods);
        this.AddOutArgumentExtensions(builder, methods);
        this.AddThrowExtensions(builder, methods);
    }

    private void AddOutArgumentExtensions(CodeBuilder builder, IGrouping<string, IMethodSymbol> methods)
    {
        var methodSymbols = methods.Where(m => m.Parameters.Any(symbol => symbol.RefKind == RefKind.Out));

        foreach (var method in methodSymbols)
        {
            builder
                .Documentation(doc => doc
                    .Summary("Configures the mock to return the specified values.")
                    .Parameter("returns", "The return value of the method.", !method.ReturnsVoid)
                    .Parameter(method.Parameters.Where(t => t.RefKind == RefKind.Out), symbol => "out_" + symbol.Name, argument => $"Value to set for the out argument {argument.Name}")
                    .Returns("The updated configuration object."));

            var arguments = method.Parameters.Where(t => t.RefKind == RefKind.Out).ToString(p => $"{p.Type.ToDisplayString()} out_{p.Name}");
            if (!method.ReturnsVoid)
                arguments = method.ReturnType.ToDisplayString() + " returns, " + arguments;

            builder.AddConfigLambda(this.context, method, [arguments], configScope =>
            {
                    var parameters = GetParameterInfos(method);

                    var parameterList = parameters.ToString(p => $"{p.OutString}{p.Type} {p.Name}");

                    configScope.Add($"this.{method.Name}(call: ({this.methodDelegateName[method]})(({parameterList}) => {{").Indent();

                    foreach (var symbol in method.Parameters.Where(t => t.RefKind == RefKind.Out))
                    {
                        configScope.Add($"{symbol.Name} = out_{symbol.Name};");
                    }

                    if (!method.ReturnsVoid)
                        configScope.Add("return returns;");

                    configScope.Unindent().Add("}));");
            });
        }
    }

    private void AddReturnsExtensions(CodeBuilder result, IGrouping<string, IMethodSymbol> methods)
    {
        var methodSymbols = methods.Where(m => !m.ReturnsVoid && !m.Parameters.Any(symbol => symbol.RefKind == RefKind.Out));
        var candidates = methodSymbols.ToLookup(t => t.ReturnType, SymbolEqualityComparer.Default);
        foreach (var candidate in candidates)
        {
            var seeString = string.Join(", ", candidate.Select(t => t.ToSeeCRef()));

            result.AddLineBreak();
            result.Documentation(doc => doc
                .Summary("Configures the mock to return a specific value, regardless of the provided arguments.", $"Use this to quickly define a fixed return result for {seeString}.")
                .Parameter("returns", "The fixed value that should be returned by the mock.")
                .Returns("The updated configuration object."));

            var first = candidate.First();
            var firstReturnType = (ITypeSymbol)candidate.Key!;

            var returnType = firstReturnType.ToString();
            if (firstReturnType is { TypeKind: TypeKind.TypeParameter, ContainingSymbol: IMethodSymbol })
            {
                returnType = "System.Object";
            }

            result.AddConfigExtension(this.context, first, [returnType + " returns"], builder =>
            {
                foreach (var m in candidate)
                {
                    var parameters = GetParameterInfos(m);

                    var parameterList = parameters.ToString(p => $"{p.OutString}{p.Type} _");

                    builder.Add($"this.{m.Name}(call: ({this.methodDelegateName[m]})(({parameterList}) => returns));");
                }
            });
        }
    }

    private void AddTaskReturnsExtensions(CodeBuilder result, IGrouping<string, IMethodSymbol> methods)
    {
        var methodSymbols = methods.Where(m => !m.ReturnsVoid && !m.Parameters.Any(symbol => symbol.RefKind == RefKind.Out));
        var candidates = methodSymbols.ToLookup(t => t.ReturnType, SymbolEqualityComparer.Default);
        foreach (var candidate in candidates)
        {
            var firstReturnType = (ITypeSymbol)candidate.Key!;

            var isGenericTask = firstReturnType.IsGenericTask();
            var isGenericValueTask = firstReturnType.IsGenericValueTask();
            if (isGenericTask || isGenericValueTask)
            {
                var seeString = string.Join(", ", candidate.Select(t => t.ToSeeCRef()));
                var first = candidate.First();
                var genericType = ((INamedTypeSymbol)firstReturnType).TypeArguments.First();

                result.AddLineBreak();
                result.Documentation(doc => doc
                    .Summary("Configures the mock to return a specific value as a <see cref=\"System.Threading.Tasks.Task{T}\"/>, regardless of the provided arguments.", $"Use this to quickly define a fixed return result for {seeString}.")
                    .Parameter("returnAsTasks", $"The fixed {genericType.ToSeeCRef()} that should be returned by the mock wrapped in a <see cref=\"System.Threading.Tasks.Task{{T}}\"/>")
                    .Returns("The updated configuration object."));

                result.AddConfigExtension(this.context, first, [genericType + " returnAsTasks"], builder =>
                {
                    foreach (var m in candidate)
                    {
                        var parameters = GetParameterInfos(m);

                        var parameterList = parameters.ToString(p => $"{p.OutString}{p.Type} _");
                        if (isGenericTask)
                        {
                            builder.Add($"this.{m.Name}(call: ({parameterList}) => System.Threading.Tasks.Task.FromResult(returnAsTasks));");
                        }

                        if (isGenericValueTask)
                        {
                            builder.Add($"this.{m.Name}(call: ({parameterList}) => System.Threading.Tasks.ValueTask.FromResult(returnAsTasks));");
                        }
                    }
                });
            }
        }
    }

    private void AddReturnValuesExtensions(CodeBuilder result, IGrouping<string, IMethodSymbol> source)
    {
        var methodSymbols = source.Where(m => !m.ReturnsVoid && !m.Parameters.Any(symbol => symbol.RefKind == RefKind.Out));
        var candidates = methodSymbols.ToLookup(t => t.Name + ":" + t.ReturnType);
        foreach (var candidate in candidates)
        {
            var seeString = string.Join(", ", candidate.Select(t => t.ToSeeCRef()));

            result.AddLineBreak();
            result.Documentation(doc => doc
                .Summary("Configures the mock to return one of the specific values disregarding the arguments.", $"Configures {seeString}")
                .Parameter("returnValues", "The values that should be returned in order. If the values are depleted <see cref=\"System.InvalidOperationException\"/>  is thrown.")
                .Returns("The updated configuration object."));

            var returnType = candidate.First().ReturnType.ToString();
            if (candidate.First().ReturnType.TypeKind == TypeKind.TypeParameter && candidate.First().ReturnType.ContainingSymbol is IMethodSymbol)
            {
                returnType = "System.Object";
            }

            returnType = "System.Collections.Generic.IEnumerable<" + returnType + ">";

            result.AddConfigExtension(this.context, candidate.First(), [returnType + " returnValues"], builder =>
            {
                var index = 0;
                foreach (var m in candidate)
                {
                    index++;
                    var index1 = index;
                    var parameters = GetParameterInfos(m);
                    var parameterList = parameters.ToString(p => $"{p.OutString}{p.Type} _");

                    builder.Add($"var {m.Name}{index1}_Values = returnValues.GetEnumerator();");
                    builder.Scope($"this.{m.Name}(call: ({this.methodDelegateName[m]})(({parameterList}) => ", lambdaScope => lambdaScope
                            .Scope($"if({m.Name}{index1}_Values.MoveNext())", conditionScope => conditionScope
                                .Add($"return {m.Name}{index1}_Values.Current;")
                            )
                            .Add(m.BuildNotMockedException()))
                        .Add("));");
                }
            });
        }
    }

    private void AddNoReturnValueExtensions(CodeBuilder result, IGrouping<string, IMethodSymbol> source)
    {
        var methodSymbols = source.Where(t => (t.ReturnsVoid || t.ReturnType.ToString() == "System.Threading.Tasks.Task" || t.ReturnType.ToString() == "System.Threading.Tasks.ValueTask") && !t.Parameters.Any(s => s.RefKind == RefKind.Out));
        var candidates = methodSymbols.ToLookup(t => t.Name);
        foreach (var candidate in candidates)
        {
            var seeString = string.Join(", ", candidate.Select(t => t.ToSeeCRef()));

            result.AddLineBreak();
            result.Documentation(doc => doc
                .Summary("Configures the mock to accept any call to methods not returning values.", $"Configures {seeString}")
                .Returns("The updated configuration object."));

            result.AddConfigExtension(this.context, candidate.First(), [], builder =>
            {
                foreach (var m in candidate)
                {
                    var parameters = GetParameterInfos(m);
                    var parameterList = parameters.ToString(p => $"{p.OutString}{p.Type} _");

                    var str = m.ReturnType.ToString() switch
                    {
                        "void" => $"this.{m.Name}(call: ({this.methodDelegateName[m]})(({parameterList}) => {{}}));",
                        "System.Threading.Tasks.Task" => $"this.{m.Name}(call: ({this.methodDelegateName[m]})(({parameterList}) => System.Threading.Tasks.Task.CompletedTask));",
                        "System.Threading.Tasks.ValueTask" => $"this.{m.Name}(call: ({this.methodDelegateName[m]})(({parameterList}) => System.Threading.Tasks.ValueTask.CompletedTask));",
                        _ => ""
                    };

                    builder.Add(str);
                }
            });
        }
    }

    private void AddThrowExtensions(CodeBuilder result, IGrouping<string, IMethodSymbol> methods)
    {
        foreach (var methodGroup in methods.ToLookup(t => t.Name))
        {
            var seeString = string.Join(", ", methodGroup.Select(t => t.ToSeeCRef()));

            result.AddLineBreak();
            result.Documentation(doc => doc
                .Summary("Configures the mock to throw the specified exception when the method is called.", $"Configures {seeString}")
                .Parameter("throws", "The exception to be thrown when the method is called.")
                .Returns("The updated configuration object."));

            result.AddConfigExtension(this.context, methodGroup.First(), ["Exception throws"], builder =>
            {
                foreach (var method in methodGroup)
                {
                    var parameters = GetParameterInfos(method);

                    var parameterList = parameters.ToString(p => $"{p.OutString}{p.Type} _");

                    builder.Add($"this.{method.Name}(call: ({this.methodDelegateName[method]})(({parameterList}) => throw throws));");
                }
            });
        }
    }

    private record DelegateInfo(string Name, string Type, string Parameters);

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
