namespace SweetMock.Builders.MemberBuilders;

using Generation;
using Utils;

internal partial class MethodBuilder
{
    private void BuildConfigExtensions(CodeBuilder builder, IMethodSymbol[] methods)
    {
        this.Call(builder, methods);
        this.Returns(builder, methods);
        this.TaskReturns(builder, methods);
        this.ReturnValues(builder, methods);
        this.NoReturnValue(builder, methods);
        this.OutArgument(builder, methods);
        this.Throw(builder, methods);
    }

    private void Call(CodeBuilder builder, IEnumerable<IMethodSymbol> methods)
    {
        var methodCount = 1;
        foreach (var method in methods)
        {
            var (delegateName, delegateType, delegateParameters) = method.GetDelegateInfo(methodCount);
            var name = method.Name;
            var functionPointer = methodCount == 1 ? $"_{name}" : $"_{name}_{methodCount}";

            builder
                .Documentation($"Delegate for mocking calls to {method.ToSeeCRef()}.")
                .Add($"public delegate {delegateType} {delegateName}({delegateParameters});")
                .BR()
                .Documentation(doc => doc
                    .Summary($"Configures the mock to execute the specified action when calling {method.ToSeeCRef()}.")
                    .Parameter("call", "The action or function to execute when the method is called.")
                    .Returns("The updated configuration object."))
                .AddConfigMethod(this.context, name, [$"{delegateName} call"], methodScope => methodScope
                    .Add($"target.{functionPointer} = call;"));

            methodCount++;
        }
    }

    private void Returns(CodeBuilder result, IEnumerable<IMethodSymbol> methods)
    {
        var methodSymbols = methods.Where(m => !m.ReturnsVoid && !m.HasOutParameters());
        var candidates = methodSymbols.ToLookup(t => t.ReturnType, SymbolEqualityComparer.Default).Where(candidate => !candidate.IsReturnTypeDerivedFromGeneric());

        foreach (var candidate in candidates)
        {
            var first = candidate.First();
            var returnType = ReturnType(candidate);

            result
                .BR()
                .Documentation(doc => doc
                    .Summary("Configures the mock to return a specific value, regardless of the provided arguments.", $"Use this to quickly define a fixed return result for {candidate.ToSeeCRef()}.")
                    .Parameter("returns", "The fixed value that should be returned by the mock.")
                    .Returns("The updated configuration object."))
                .AddConfigExtension(this.context, first, [returnType + " returns"], builder => builder
                    .AddMultiple(candidate, m => $"this.{m.Name}(call: ({this.methodDelegateName[m]})(({ParameterList(m)}) => returns));"));
        }

        return;

        string ParameterList(IMethodSymbol m) => m.GetParameterInfos().ToString(p => $"{p.OutString}{p.Type} _");

        string ReturnType(IGrouping<ISymbol?, IMethodSymbol> candidate)
        {
            var firstReturnType = (ITypeSymbol)candidate.Key!;

            return firstReturnType is { TypeKind: TypeKind.TypeParameter, ContainingSymbol: IMethodSymbol } ? "System.Object" : firstReturnType.ToString();
        }
    }

    private void TaskReturns(CodeBuilder result, IEnumerable<IMethodSymbol> methods)
    {
        var methodSymbols = methods.Where(m => !m.ReturnsVoid && !m.HasOutParameters());
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

            var first = candidate.First();
            var genericType = ((INamedTypeSymbol)firstReturnType).TypeArguments.First();

            result
                .BR()
                .Documentation(doc => doc
                    .Summary("Configures the mock to return a specific value as a <see cref=\"System.Threading.Tasks.Task{T}\"/>, regardless of the provided arguments.", $"Use this to quickly define a fixed return result for {candidate.ToSeeCRef()}.")
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

    private void ReturnValues(CodeBuilder result, IEnumerable<IMethodSymbol> methods)
    {
        var methodSymbols = methods.Where(m => !m.ReturnsVoid && !m.HasOutParameters());
        var byReturnType = methodSymbols.ToLookup(t => t.ReturnType, SymbolEqualityComparer.Default);

        foreach (var grouping in byReturnType.Where(t => !t.IsReturnTypeDerivedFromGeneric()))
        {
            var returnType = ReturnType(grouping);

            result
                .BR()
                .Documentation(doc => doc
                    .Summary("Configures the mock to return one of the specific values disregarding the arguments.", $"Configures {grouping.ToSeeCRef()}")
                    .Parameter("returnValues", "The values that should be returned in order. If the values are depleted <see cref=\"global::SweetMock.NotExplicitlyMockedException\"/>  is thrown.")
                    .Returns("The updated configuration object."))
                .AddConfigExtension(this.context, grouping.First(), [returnType + " returnValues"], builder =>
                {
                    var index = 0;
                    foreach (var method in grouping)
                    {
                        index++;
                        var index1 = index;
                        var parameters = method.GetParameterInfos();
                        var parameterList = parameters.ToString(p => $"{p.OutString}{p.Type} _");

                        if (index > 1)
                        {
                            builder.BR();
                        }

                        builder
                            .Add($"var {method.Name}{index1}_Values = returnValues.GetEnumerator();")
                            .Scope($"this.{method.Name}(call: ({this.methodDelegateName[method]})(({parameterList}) => ", lambdaScope => lambdaScope
                                .Scope($"if({method.Name}{index1}_Values.MoveNext())", conditionScope => conditionScope
                                    .Add($"return {method.Name}{index1}_Values.Current;")
                                )
                                .Add($"throw new global::SweetMock.NotExplicitlyMockedException(\"{method.Name}\", this.target._sweetMockInstanceName);"))
                            .Add("));");
                    }
                });
        }

        return;

        string ReturnType(IGrouping<ISymbol?, IMethodSymbol> method)
        {
            var returnType = method.Key!.ToString();
            if (method.First().ReturnType.TypeKind == TypeKind.TypeParameter && method.First().ReturnType.ContainingSymbol is IMethodSymbol)
            {
                returnType = "System.Object";
            }

            return "System.Collections.Generic.IEnumerable<" + returnType + ">";
        }
    }

    private void NoReturnValue(CodeBuilder result, IEnumerable<IMethodSymbol> methods)
    {
        var methodSymbols = methods
            .Where(t => (t.ReturnsVoid || t.ReturnType.ToString() == "System.Threading.Tasks.Task" ||
                         t.ReturnType.ToString() == "System.Threading.Tasks.ValueTask") &&
                        !t.HasOutParameters()).ToArray();

        if (methodSymbols.Length == 0) return;

        result
            .BR()
            .Documentation(doc => doc
                .Summary("Configures the mock to accept any call to methods not returning values.",
                    $"Configures {methodSymbols.ToSeeCRef()}")
                .Returns("The updated configuration object."))
            .AddConfigExtension(this.context, methodSymbols.First(), [], builder =>
            {
                foreach (var method in methodSymbols)
                {
                    var parameterList = method.GetParameterInfos().ToString(p => $"{p.OutString}{p.Type} _");

                    var str = method.ReturnType.ToString() switch
                    {
                        "void" => $"this.{method.Name}(call: ({this.methodDelegateName[method]})(({parameterList}) => {{}}));",
                        "System.Threading.Tasks.Task" => $"this.{method.Name}(call: ({this.methodDelegateName[method]})(({parameterList}) => System.Threading.Tasks.Task.CompletedTask));",
                        "System.Threading.Tasks.ValueTask" => $"this.{method.Name}(call: ({this.methodDelegateName[method]})(({parameterList}) => System.Threading.Tasks.ValueTask.CompletedTask));",
                        _ => ""
                    };

                    builder.Add(str);
                }
            });
    }

    private void OutArgument(CodeBuilder builder, IEnumerable<IMethodSymbol> methods)
    {
        foreach (var method in methods)
        {
            var outParameters = method.OutParameters().ToArray();
            if (outParameters.Length == 0)
            {
                continue;
            }

            var arguments = Arguments(outParameters, method);

            var parameterList = method.GetParameterInfos().ToString(p => $"{p.OutString}{p.Type} {p.Name}");

            builder
                .BR()
                .Documentation(doc => doc
                    .Summary("Configures the mock to return the specified values.")
                    .ParameterIf(!method.ReturnsVoid, "returns", "The return value of the method.")
                    .Parameters(outParameters, symbol => "out_" + symbol.Name, argument => $"Value to set for the out argument {argument.Name}")
                    .Returns("The updated configuration object."))
                .AddConfigLambda(this.context, method, [arguments], configScope => configScope
                    .Add($"this.{method.Name}(call: ({this.methodDelegateName[method]})(({parameterList}) => {{")
                    .Indent(indentScope => indentScope
                        .AddMultiple(outParameters, symbol => $"{symbol.Name} = out_{symbol.Name};")
                        .AddIf(!method.ReturnsVoid, () => "return returns;"))
                    .Add("}));"));
        }

        return;

        string Arguments(IParameterSymbol[] outParameters, IMethodSymbol method)
        {
            var arguments = outParameters.ToString(p => $"{p.Type.ToDisplayString()} out_{p.Name}");
            if (!method.ReturnsVoid)
            {
                arguments = method.ReturnType.ToDisplayString() + " returns, " + arguments;
            }

            return arguments;
        }
    }

    private void Throw(CodeBuilder result, IMethodSymbol[] methods)
    {
        result
            .BR()
            .Documentation(doc => doc
                .Summary("Configures the mock to throw the specified exception when the method is called.", $"Configures {methods.ToSeeCRef()}")
                .Parameter("throws", "The exception to be thrown when the method is called.")
                .Returns("The updated configuration object."))
            .AddConfigExtension(this.context, methods.First(), ["System.Exception throws"], builder =>
                builder
                    .AddMultiple(methods, method => $"this.{method.Name}(call: ({this.methodDelegateName[method]})(({GenerateParameterString(method)}) => throw throws));")
            )
            .BR();

        return;

        string GenerateParameterString(IMethodSymbol method) => method.GetParameterInfos().ToString(p => $"{p.OutString}{p.Type} _");
    }

    private static bool IsGenericTask(ITypeSymbol type) =>
        type.ToString().StartsWith("System.Threading.Tasks.Task<") &&
        ((INamedTypeSymbol)type).TypeArguments.Length > 0;

    private static bool IsGenericValueTask(ITypeSymbol type) =>
        type.ToString().StartsWith("System.Threading.Tasks.ValueTask<") &&
        ((INamedTypeSymbol)type).TypeArguments.Length > 0;
}
