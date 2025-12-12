namespace SweetMock.Builders.MemberBuilders;

using Exceptions;
using Generation;

/// <summary>
///     Represents a builder for methods, implementing the ISymbolBuilder interface.
/// </summary>
internal partial class MethodBuilder(IEnumerable<IMethodSymbol> methods, MockInfo mock)
{
    public static void Render(CodeBuilder classScope, MockInfo context)
    {
        var methods = context.Candidates.OfType<IMethodSymbol>().Where(t => t.MethodKind == MethodKind.Ordinary);

        var methodBuilder = new MethodBuilder(methods, context);
        methodBuilder.Render(classScope);
    }

    private ILookup<string, MethodMetadata> AllMethods { get; } = MethodGroups(methods, mock);

    private static ILookup<string, MethodMetadata> MethodGroups(IEnumerable<IMethodSymbol> methods, MockInfo context)
    {
        var methodGroups = methods.ToLookup(t => t.Name, symbol => new MethodMetadata(symbol, context));
        foreach (var grouping in methodGroups)
        {
            var index = 1;
            foreach (var method in grouping)
            {
                method.Initialize(index);
                index++;
            }
        }

        return methodGroups;
    }

    private void Render(CodeBuilder classScope)
    {
        foreach (var methodGroup in this.AllMethods)
        {
            classScope.Region($"Method : {methodGroup.Key}", builder =>
            {
                CreateLogArgumentsRecord(classScope, methodGroup);

                foreach (var method in methodGroup)
                {
                    this.Build(builder, method);
                }

                classScope.AddToConfig(methodGroup.First().Mock, config => this.BuildConfigExtensions(config, methodGroup));
            });
        }
    }

    private static void CreateLogArgumentsRecord(CodeBuilder classScope, IGrouping<string, MethodMetadata> methodGroup)
    {
        var arguments = methodGroup.SelectMany(t => t.Parameters).ToLookup(t => t.Name);
        var args = string.Join(", ", arguments.Select(t => t.GenerateArgumentDeclaration()));

        classScope
            .Add($"public record {methodGroup.Key}_Arguments(")
            .Indent(scope => scope
                .Add("global::System.String? InstanceName,")
                .Add("global::System.String MethodSignature" + (arguments.Count != 0 ? "," : ""))
                .Add(args))
            .Add($") : ArgumentBase(_containerName, \"{methodGroup.Key}\", MethodSignature, InstanceName);")
            .BR();
    }

    /// <summary>
    ///     Builds a method and adds it to the code builder.
    /// </summary>
    /// <param name="classScope">The code builder for the class scope.</param>
    /// <param name="method">Metadata for the method to build.</param>
    /// <returns>True if the method was built; otherwise, false.</returns>
    private void Build(CodeBuilder classScope, MethodMetadata method)
    {
        if (method.ReturnsByRef)
        {
            throw new RefReturnTypeNotSupportedException(method);
        }

        var valueSetters = string.Join("", method.Parameters.Where(t => t.RefKind == RefKind.None).Select(t => $", {t.Name} : {t.Name}"));

        var signature = method.IsInInterface ?
            $"{method.ReturnTypeString} {method.ContainingSymbol}.{method.Name}{method.GenericString}({method.NamedParameterString})"
            : $"{method.AccessibilityString} override {method.ReturnTypeString} {method.Name}{method.GenericString}({method.NamedParameterString})";

        classScope
            .Documentation($"Delegate for mocking calls to {method.ToSeeCRef}.")
            .Add($"public delegate {method.DelegateType} {method.DelegateName}({method.ParametersString});")
            .BR()
            .Add($"private {method.DelegateName} {method.FunctionPointer} {{get;set;}} = null!;")
            .BR()
            .Scope(signature, methodScope => methodScope
                .Add($"this._log(new {method.Name}_Arguments(this._sweetMockInstanceName, \"{method.FullName}\"{valueSetters}));")
                .AddIf(method.ReturnsVoid, () => $"this.{method.FunctionPointer}.Invoke({method.NameList});")
                .AddIf(!method.ReturnsVoid, () => $"return ({method.ReturnTypeString})this.{method.FunctionPointer}.Invoke({method.NameList});"))
            .BR();
    }
}
