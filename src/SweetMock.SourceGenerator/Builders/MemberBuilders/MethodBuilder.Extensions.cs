namespace SweetMock.Builders.MemberBuilders;

using Generation;
using Utils;

internal partial class MethodBuilder
{
    private void BuildConfigExtensions(CodeBuilder builder, IGrouping<string, MethodMetadata> info)
    {
        this.Call(builder, info);
        this.Throw(builder, info);
        this.Returns(builder, info);
        this.ReturnAsTasks(builder, info);
        this.ReturnValues(builder, info);
        this.NoReturnValue(builder, info);
        this.OutArgument(builder, info);
    }

    private void Call(CodeBuilder builder, IGrouping<string, MethodMetadata> methods)
    {
        foreach (var method in methods)
        {
            builder
                .Documentation(doc => doc
                    .Summary($"Configures the mock to execute the specified action when calling {method.ToSeeCRef}.")
                    .Parameter("call", "The action or function to execute when the method is called.")
                    .Returns("The updated configuration object."))
                .AddConfigMethod(method.Mock, method.Name, [$"{method.DelegateName} call"], methodScope => methodScope
                    .Add($"target.{method.FunctionPointer} = call;"))
                .BR();
        }
    }

    private void Throw(CodeBuilder result, IGrouping<string, MethodMetadata> methods) => result
        .Documentation(doc => doc
            .Summary("Configures the mock to throw the specified exception when the method is called.", $"Configures {methods.ToSeeCRef()}")
            .Parameter("throws", "The exception to be thrown when the method is called.")
            .Returns("The updated configuration object."))
        .AddConfigMethod(methods.First().Mock, methods.Key, ["System.Exception throws"], builder => builder
            .AddMultiple(methods, method => $"this.{method.Name}(call: ({method.DelegateName})(({method.ParametersString}) => throw throws));")
        )
        .BR();

    private void Returns(CodeBuilder result, IGrouping<string, MethodMetadata> methods)
    {
        var groupedByReturnType = methods
            .Where(m => m is { ReturnsVoid: false, HasOutParameters: false, ReturnTypeDerivedFromGeneric: false })
            .GroupByReturnType();

        foreach (var methodGroup in groupedByReturnType)
        {
            result
                .Documentation(doc => doc
                    .Summary("Configures the mock to return a specific value, regardless of the provided arguments.", $"Use this to quickly define a fixed return result for {methodGroup.ToSeeCRef()}.")
                    .Parameter("returns", "The fixed value that should be returned by the mock.")
                    .Returns("The updated configuration object."))
                .AddConfigMethod(methods.First().Mock, methods.Key, [methodGroup.First().ReturnStringWithoutGeneric + " returns"], builder => builder
                    .AddMultiple(methodGroup, method => $"this.{method.Name}(call: ({method.DelegateName})(({method.ParametersString}) => returns));"))
                .BR();
        }
    }

    private void ReturnAsTasks(CodeBuilder result, IGrouping<string, MethodMetadata> methods)
    {
        var groupedByReturnType = methods
            .Where(m => !m.ReturnsVoid && !m.HasOutParameters)
            .GroupByReturnType();

        foreach (var methodGroup in groupedByReturnType)
        {
            var first = methodGroup.First();

            var returnType = first.ReturnType as INamedTypeSymbol;
            var isGenericTask = first.ReturnsGenericTask;
            var isGenericValueTask = first.ReturnsGenericValueTask;
            if (!isGenericTask && !isGenericValueTask)
            {
                continue;
            }

            var genericType = returnType!.TypeArguments.First();

            result
                .BR()
                .Documentation(doc => doc
                    .Summary("Configures the mock to return a specific value as a <see cref=\"System.Threading.Tasks.Task{T}\"/>, regardless of the provided arguments.", $"Use this to quickly define a fixed return result for {methodGroup.ToSeeCRef()}.")
                    .Parameter("returnAsTasks", $"The fixed {genericType.ToSeeCRef()} that should be returned by the mock wrapped in a <see cref=\"System.Threading.Tasks.Task{{T}}\"/>")
                    .Returns("The updated configuration object."))
                .AddConfigMethod(first.Mock, methods.Key, [genericType + " returnAsTasks"], builder =>
                {
                    foreach (var method in methodGroup)
                    {
                        if (method.ReturnsGenericTask)
                        {
                            builder.Add($"this.{method.Name}(call: ({method.ParametersString}) => System.Threading.Tasks.Task.FromResult(returnAsTasks));");
                        }

                        if (method.ReturnsGenericValueTask)
                        {
                            builder.Add($"this.{method.Name}(call: ({method.ParametersString}) => System.Threading.Tasks.ValueTask.FromResult(returnAsTasks));");
                        }
                    }
                });
        }
    }

    private void ReturnValues(CodeBuilder result, IGrouping<string, MethodMetadata> methods)
    {
        var byReturnType = methods.Where(m => m is { ReturnsVoid: false, HasOutParameters: false, ReturnTypeDerivedFromGeneric: false }).GroupByReturnType();

        foreach (var grouping in byReturnType)
        {
            var returnType = ReturnType(grouping);

            var first = grouping.First();

            result
                .BR()
                .Documentation(doc => doc
                    .Summary("Configures the mock to return one of the specific values disregarding the arguments.", $"Configures {grouping.ToSeeCRef()}")
                    .Parameter("returnValues", "The values that should be returned in order. If the values are depleted <see cref=\"global::SweetMock.NotExplicitlyMockedException\"/>  is thrown.")
                    .Returns("The updated configuration object."))
                .AddConfigMethod(first.Mock, first.Name, [returnType + " returnValues"], builder =>
                {
                    var index = 0;
                    foreach (var method in grouping)
                    {
                        index++;
                        var index1 = index;
                        var parameterList = method.ParametersString;

                        if (index > 1)
                        {
                            builder.BR();
                        }

                        builder
                            .Add($"var {method.Name}{index1}_Values = returnValues.GetEnumerator();")
                            .Scope($"this.{method.Name}(call: ({method.DelegateName})(({parameterList}) => ", lambdaScope => lambdaScope
                                .Scope($"if({method.Name}{index1}_Values.MoveNext())", conditionScope => conditionScope
                                    .Add($"return {method.Name}{index1}_Values.Current;")
                                )
                                .Add($"throw new global::SweetMock.NotExplicitlyMockedException(\"{method.Name}\", this.target._sweetMockInstanceName);"))
                            .Add("));");
                    }
                });
        }

        return;

        string ReturnType(IGrouping<ISymbol?, MethodMetadata> method)
        {
            var returnType = method.Key!.ToDisplayString(Format.SignatureOnlyFormat);
            if (method.First().ReturnType.TypeKind == TypeKind.TypeParameter && method.First().ReturnType.ContainingSymbol is IMethodSymbol)
            {
                returnType = "global::System.Object";
            }

            return "global::System.Collections.Generic.IEnumerable<" + returnType + ">";
        }
    }

    private void NoReturnValue(CodeBuilder result, IGrouping<string, MethodMetadata> methods)
    {
        var methodSymbols = methods
            .Where(t => (t.ReturnsVoid || t.ReturnTypeString == "global::System.Threading.Tasks.Task" ||
                         t.ReturnTypeString == "global::System.Threading.Tasks.ValueTask") &&
                        !t.HasOutParameters).ToArray();

        if (methodSymbols.Length != 0)
        {
            result
                .BR()
                .Documentation(doc => doc
                    .Summary("Configures the mock to accept any call to methods not returning values.",
                        $"Configures {methodSymbols.ToSeeCRef()}")
                    .Returns("The updated configuration object."))
                .AddConfigMethod(methods.First().Mock, methods.Key, [], builder =>
                {
                    foreach (var method in methodSymbols)
                    {
                        var parameterList = method.ParametersString;

                        var str = method.ReturnTypeString switch
                        {
                            "void" => $"this.{method.Name}(call: ({method.DelegateName})(({parameterList}) => {{}}));",
                            "global::System.Threading.Tasks.Task" => $"this.{method.Name}(call: ({method.DelegateName})(({parameterList}) => System.Threading.Tasks.Task.CompletedTask));",
                            "global::System.Threading.Tasks.ValueTask" => $"this.{method.Name}(call: ({method.DelegateName})(({parameterList}) => System.Threading.Tasks.ValueTask.CompletedTask));",
                            _ => ""
                        };

                        builder.Add(str);
                    }
                });
        }
    }

    private void OutArgument(CodeBuilder builder, IGrouping<string, MethodMetadata> methods)
    {
        foreach (var method in methods.Where(t => t.HasOutParameters))
        {
            var outParameters = method.Parameters.Where(t => t.RefKind == RefKind.Out).ToArray();

            var arguments = outParameters.Combine(p => $"{p.Type.ToDisplayString(Format.ToFullNameFormatWithGlobal)} out_{p.Name}");
            if (!method.ReturnsVoid)
            {
                arguments = method.ReturnTypeString + " returns, " + arguments;
            }

            builder
                .BR()
                .Documentation(doc => doc
                    .Summary("Configures the mock to return the specified values.")
                    .ParameterIf(!method.ReturnsVoid, "returns", "The return value of the method.")
                    .Parameters(outParameters, parameter => $"out_{parameter.Name}", argument => $"Value to set for the out argument {argument.Name}")
                    .Returns("The updated configuration object."))
                .AddConfigLambda(methods.First().Mock, method.Name, [arguments], configScope => configScope
                    .Add($"this.{method.Name}(call: ({method.DelegateName})(({method.ParametersString}) => {{")
                    .Indent(indentScope => indentScope
                        .AddMultiple(outParameters, symbol => $"{symbol.Name} = out_{symbol.Name};")
                        .AddIf(!method.ReturnsVoid, () => "return returns;"))
                    .Add("}));"));
        }
    }
}
