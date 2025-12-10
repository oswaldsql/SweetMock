namespace SweetMock.Builders.MemberBuilders;

using Exceptions;
using Generation;
using Utils;

/// <summary>
///     Represents a builder for methods, implementing the ISymbolBuilder interface.
/// </summary>
internal partial class MethodBuilder
{
    public ILookup<string, MethodMetadata> AllMethods { get; set; }

    private MethodBuilder(IEnumerable<IMethodSymbol> methods, MockContext context)
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

        this.AllMethods = methodGroups;
    }

    public static void Render(CodeBuilder classScope, MockContext context, IEnumerable<IMethodSymbol> methods)
    {
        var methodBuilder = new MethodBuilder(methods, context);
        methodBuilder.Render(classScope);
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

                classScope.AddToConfig(methodGroup.First().Context, config => this.BuildConfigExtensions(config, methodGroup));
            });
        }
    }

    private static void CreateLogArgumentsRecord(CodeBuilder classScope, IGrouping<string, MethodMetadata> methodGroup)
    {
        var arguments = methodGroup.SelectMany(t => t.Symbol.Parameters).ToLookup(t => t.Name);
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

        var valueSetters = string.Join("", method.Symbol.Parameters.Where(t => t.RefKind == RefKind.None).Select(t => $", {t.Name} : {t.Name}"));

        classScope
            .Documentation($"Delegate for mocking calls to {method.ToSeeCRef}.")
            .Add($"public delegate {method.DelegateType} {method.DelegateName}({method.DelegateParameters});")
            .BR()
            .Add($"private {method.DelegateName} {method.FunctionPointer} {{get;set;}} = null!;")
            .BR()
            .Scope(method.Signature, methodScope => methodScope
                .Add($"this._log(new {method.Name}_Arguments(this._sweetMockInstanceName, \"{method.FullName}\"{valueSetters}));")
                .Add($"{method.ReturnString}{method.CastString}this.{method.FunctionPointer}.Invoke({method.NameList});"))
            .BR();
    }
}
