namespace SweetMock.Builders.MemberBuilders;

using Exceptions;
using Generation;
using Utils;

/// <summary>
///     Represents a builder for methods, implementing the ISymbolBuilder interface.
/// </summary>
internal partial class MethodBuilder
{
    private readonly MockContext context;
    private readonly Dictionary<IMethodSymbol, string> methodDelegateName = new(SymbolEqualityComparer.Default);
    private readonly ILookup<string, IMethodSymbol> methodGroups;

    private MethodBuilder(IEnumerable<IMethodSymbol> methods, MockContext context)
    {
        this.context = context;
        this.methodGroups = methods.ToLookup(t => t.Name);
    }

    public static void Render(CodeBuilder classScope, MockContext context, IEnumerable<IMethodSymbol> methods)
    {
        var methodBuilder = new MethodBuilder(methods, context);
        methodBuilder.Render(classScope);
    }

    private void Render(CodeBuilder classScope)
    {
        foreach (var methodGroup in this.methodGroups)
        {
            classScope.Region($"Method : {methodGroup.Key}", builder =>
            {
                CreateLogArgumentsRecord(classScope, methodGroup);

                var methodCount = 1;
                foreach (var symbol in methodGroup)
                {
                    this.Build(builder, symbol, methodCount);
                    methodCount++;
                }

                classScope.AddToConfig(this.context, config => this.BuildConfigExtensions(config, methodGroup.ToArray()));
            });
        }
    }

    private static void CreateLogArgumentsRecord(CodeBuilder classScope, IGrouping<string, IMethodSymbol> methodGroup)
    {
        var arguments = methodGroup.SelectMany(t => t.Parameters).ToLookup(t => t.Name);
        var args = string.Join(", ", arguments.Select(GetArgString));

        classScope
            .Add($"public record {methodGroup.Key}_Arguments(")
            .Indent(scope => scope
                .Add("global::System.String? InstanceName,")
                .Add("global::System.String MethodSignature" + (arguments.Count != 0 ? "," : ""))
                .Add(args))
            .Add($") : ArgumentBase(_containerName, \"{methodGroup.Key}\", MethodSignature, InstanceName);")
            .BR();
    }

    private static string GetArgString(IGrouping<string, IParameterSymbol> argument)
    {
        string argString;
        if (argument.Select(t => t.Type).Distinct(SymbolEqualityComparer.Default).Count() == 1)
        {
            var firstType = argument.First().Type;
            if (firstType is INamedTypeSymbol namedType)
            {
                if (namedType.DelegateInvokeMethod != null)
                {
                    // Delegate types like System.Func<T> will be written as global::System.Object
                    argString = $"global::System.Object? {argument.Key} = null";
                }
                else if (namedType.TypeArguments.Length > 0 || namedType.IsGenericType)
                {
                    // Generic types with nullable annotation and fully qualified format
                    argString = $"{namedType.WithNullableAnnotation(NullableAnnotation.Annotated).ToDisplayString(MethodBuilderHelpers.CustomSymbolDisplayFormat)} {argument.Key} = null";
                }
                else
                {
                    // Regular case
                    argString = $"{firstType.ToDisplayString(MethodBuilderHelpers.CustomSymbolDisplayFormat)}? {argument.Key} = null";
                }
            }
            else
            if (firstType is ITypeParameterSymbol)
            {
                argString = $"global::System.Object? {argument.Key} = null";
            }
            else
            {
                argString = $"{firstType.WithNullableAnnotation(NullableAnnotation.Annotated).ToDisplayString(MethodBuilderHelpers.CustomSymbolDisplayFormat)}? {argument.Key} = null";
            }
        }
        else
        {
            argString = $"object? {argument.Key} = null";
        }

        return argString;
    }

    /// <summary>
    ///     Builds a method and adds it to the code builder.
    /// </summary>
    /// <param name="classScope">The code builder for the class scope.</param>
    /// <param name="methodSymbol">The method symbol to build the method from.</param>
    /// <param name="methodCount">The count of methods built so far.</param>
    /// <returns>True if the method was built; otherwise, false.</returns>
    private void Build(CodeBuilder classScope, IMethodSymbol methodSymbol, int methodCount)
    {
        if (methodSymbol.ReturnsByRef)
        {
            throw new RefReturnTypeNotSupportedException(methodSymbol, methodSymbol.ContainingType);
        }

        var (methodParameters, nameList) = methodSymbol.ParameterStrings();

        var (name, returnType, returnString) = methodSymbol.MethodMetadata();

        var (containingSymbol, accessibilityString, overrideString) = methodSymbol.Overwrites();

        var (delegateName, _, _) = methodSymbol.GetDelegateInfo(methodCount);
        this.methodDelegateName.Add(methodSymbol, delegateName);

        var functionPointer = methodCount == 1 ? $"_{name}" : $"_{name}_{methodCount}";

        var genericString = methodSymbol.GenericString();
        var castString = methodSymbol is { IsGenericMethod: true, ReturnsVoid: false } ? " (" + returnType + ") " : "";

        var signature = $"{accessibilityString}{overrideString}{returnType} {containingSymbol}{name}{genericString}({methodParameters})";

        classScope
            .Scope(signature, methodScope => methodScope
                .Add($"this._log(new {methodSymbol.Name}_Arguments(this._sweetMockInstanceName, \"{methodSymbol.ToDisplayString(MethodBuilderHelpers.SignatureOnlyFormat)}\"{string.Join("", methodSymbol.Parameters.Where(t => t.RefKind == RefKind.None).Select(t => $", {t.Name} : {t.Name}"))}));")
                .Scope($"if (this.{functionPointer} is null)", ifScope =>
                    ifScope.Add($"throw new global::SweetMock.NotExplicitlyMockedException(\"{methodSymbol.Name}\", this._sweetMockInstanceName);"))
                .Add($"{returnString}{castString}this.{functionPointer}.Invoke({nameList});")
            )
            .BR()
            .Add($"private {this.context.ConfigName}.{delegateName}? {functionPointer} {{get;set;}} = null;")
            .BR();
    }
}

internal partial class MethodBuilder
{
    internal record DelegateInfo(string Name, string Type, string Parameters);

    internal record MethodInfo(string Name, string ReturnType, string ReturnString);
}
