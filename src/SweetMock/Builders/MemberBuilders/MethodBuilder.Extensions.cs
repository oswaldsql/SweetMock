namespace SweetMock.Builders.MemberBuilders;

using Generation;
using Utils;

internal partial class MethodBuilder
{
    private void BuildConfigExtensions(CodeBuilder builder, IGrouping<string, IMethodSymbol> methods)
    {
        this.AddReturnsExtensions(builder, methods);
        this.AddTaskReturnsExtensions(builder, methods);
        this.AddReturnValuesExtensions(builder, methods);
        this.AddNoReturnValueExtensions(builder, methods);
        this.AddOutArgumentExtensions(builder, methods);
        this.AddThrowExtensions(builder, methods);
    }

    private void AddReturnsExtensions(CodeBuilder result, IGrouping<string, IMethodSymbol> methods)
    {
        var methodSymbols = methods.Where(m => !m.ReturnsVoid && !m.Parameters.Any(symbol => symbol.RefKind == RefKind.Out));
        var candidates = methodSymbols.ToLookup(t => t.ReturnType, SymbolEqualityComparer.Default);
        foreach (var candidate in candidates)
        {
            if (candidate.IsReturnTypeDerivedFromGeneric())
            {
                break;
            }

            var seeString = string.Join(", ", candidate.Select(t => t.ToSeeCRef()));

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

            result.AddConfigExtension(this.context, first, [returnType + " returns"], builder => builder
                .Add(candidate, m => $"this.{m.Name}(call: ({this.methodDelegateName[m]})(({ParameterList(m)}) => returns));"));
        }

        return;

        string ParameterList(IMethodSymbol m) => m.GetParameterInfos().ToString(p => $"{p.OutString}{p.Type} _");
    }

    private void AddTaskReturnsExtensions(CodeBuilder result, IGrouping<string, IMethodSymbol> methods)
    {
        var methodSymbols = methods.Where(m => !m.ReturnsVoid && !m.Parameters.Any(symbol => symbol.RefKind == RefKind.Out));
        var candidates = methodSymbols.ToLookup(t => t.ReturnType, SymbolEqualityComparer.Default);
        foreach (var candidate in candidates)
        {
            var firstReturnType = (ITypeSymbol)candidate.Key!;

            var isGenericTask = IsGenericTask(firstReturnType);
            var isGenericValueTask = IsGenericValueTask(firstReturnType);
            if (!isGenericTask && !isGenericValueTask)
            {
                continue;
            }

            var seeStrings = string.Join(", ", candidate.Select(t => t.ToSeeCRef()));
            var first = candidate.First();
            var genericType = ((INamedTypeSymbol)firstReturnType).TypeArguments.First();

            result
                .Documentation(doc => doc
                    .Summary("Configures the mock to return a specific value as a <see cref=\"System.Threading.Tasks.Task{T}\"/>, regardless of the provided arguments.", $"Use this to quickly define a fixed return result for {seeStrings}.")
                    .Parameter("returnAsTasks", $"The fixed {genericType.ToSeeCRef()} that should be returned by the mock wrapped in a <see cref=\"System.Threading.Tasks.Task{{T}}\"/>")
                    .Returns("The updated configuration object."))
                .AddConfigExtension(this.context, first, [genericType + " returnAsTasks"], builder =>
                {
                    foreach (var m in candidate)
                    {
                        var parameterList = m.GetParameterInfos().ToString(p => $"{p.OutString}{p.Type} _");
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

    private void AddReturnValuesExtensions(CodeBuilder result, IGrouping<string, IMethodSymbol> source)
    {
        var methodSymbols = source.Where(m => !m.ReturnsVoid && !m.Parameters.Any(symbol => symbol.RefKind == RefKind.Out));
        var candidates = methodSymbols.ToLookup(t => t.Name + ":" + t.ReturnType);
        foreach (var candidate in candidates)
        {
            if (candidate.IsReturnTypeDerivedFromGeneric())
            {
                break;
            }

            var seeString = string.Join(", ", candidate.Select(t => t.ToSeeCRef()));

            result.BR();
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
                    var parameters = m.GetParameterInfos();
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

            result.Documentation(doc => doc
                .Summary("Configures the mock to accept any call to methods not returning values.", $"Configures {seeString}")
                .Returns("The updated configuration object."));

            result.AddConfigExtension(this.context, candidate.First(), [], builder =>
            {
                foreach (var m in candidate)
                {
                    var parameters = m.GetParameterInfos();
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
            {
                arguments = method.ReturnType.ToDisplayString() + " returns, " + arguments;
            }

            builder.AddConfigLambda(this.context, method, [arguments], configScope =>
            {
                var parameters = method.GetParameterInfos();

                var parameterList = parameters.ToString(p => $"{p.OutString}{p.Type} {p.Name}");

                configScope.Add($"this.{method.Name}(call: ({this.methodDelegateName[method]})(({parameterList}) => {{").Indent();

                foreach (var symbol in method.Parameters.Where(t => t.RefKind == RefKind.Out))
                {
                    configScope.Add($"{symbol.Name} = out_{symbol.Name};");
                }

                if (!method.ReturnsVoid)
                {
                    configScope.Add("return returns;");
                }

                configScope.Unindent().Add("}));");
            });
        }
    }

    private void AddThrowExtensions(CodeBuilder result, IGrouping<string, IMethodSymbol> methods)
    {
        foreach (var methodGroup in methods.ToLookup(t => t.Name))
        {
            result
                .Documentation(doc => doc
                    .Summary("Configures the mock to throw the specified exception when the method is called.", $"Configures {SeeString(methodGroup)}")
                    .Parameter("throws", "The exception to be thrown when the method is called.")
                    .Returns("The updated configuration object."))
                .AddConfigExtension(this.context, methodGroup.First(), ["System.Exception throws"], builder =>
                    builder
                        .Add(methodGroup, method => $"this.{method.Name}(call: ({this.methodDelegateName[method]})(({GenerateParameterString(method)}) => throw throws));")
                    )
                .BR();
        }

        return;

        string GenerateParameterString(IMethodSymbol method) => method.GetParameterInfos().ToString(p => $"{p.OutString}{p.Type} _");

        string SeeString(IGrouping<string, IMethodSymbol> methodGroup) => string.Join(", ", methodGroup.Select(t => t.ToSeeCRef()));
    }

    internal static bool IsGenericTask(ITypeSymbol type) =>
        type.ToString().StartsWith("System.Threading.Tasks.Task<") &&
        ((INamedTypeSymbol)type).TypeArguments.Length > 0;

    internal static bool IsGenericValueTask(ITypeSymbol type) =>
        type.ToString().StartsWith("System.Threading.Tasks.ValueTask<") &&
        ((INamedTypeSymbol)type).TypeArguments.Length > 0;
}
