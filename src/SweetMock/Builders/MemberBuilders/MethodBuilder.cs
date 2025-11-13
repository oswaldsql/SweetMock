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

        var (delegateName, delegateType, delegateParameters) = methodSymbol.GetDelegateInfo(methodCount);
        this.methodDelegateName.Add(methodSymbol, delegateName);

        var functionPointer = methodCount == 1 ? $"_{name}" : $"_{name}_{methodCount}";

        var genericString = methodSymbol.GenericString();
        var castString = methodSymbol is { IsGenericMethod: true, ReturnsVoid: false } ? " (" + returnType + ") " : "";

        var signature = $"{accessibilityString}{overrideString}{returnType} {containingSymbol}{name}{genericString}({methodParameters})";

        classScope
            .Scope(signature, methodScope => methodScope
                .BuildLogSegment(this.context, methodSymbol)
                .Scope($"if (this.{functionPointer} is null)", ifScope =>
                    ifScope.Add($"throw new SweetMock.NotExplicitlyMockedException(\"{methodSymbol.Name}\", _sweetMockInstanceName);"))
                .Add($"{returnString}{castString}this.{functionPointer}.Invoke({nameList});")
            )
            .AddLineBreak()
            .Add($"private {this.context.ConfigName}.{delegateName}? {functionPointer} {{get;set;}} = null;")
            .AddLineBreak()
            .AddToConfig(this.context, config => config
                .Documentation($"Delegate for mocking calls to {methodSymbol.ToSeeCRef()}.")
                .Add($"public delegate {delegateType} {delegateName}({delegateParameters});")
                .AddLineBreak()
                .Documentation(doc => doc
                    .Summary($"Configures the mock to execute the specified action when calling {methodSymbol.ToSeeCRef()}.")
                    .Parameter("call", "The action or function to execute when the method is called.")
                    .Returns("The updated configuration object."))
                .AddConfigMethod(this.context, name, [$"{delegateName} call"], methodScope => methodScope
                    .Add($"target.{functionPointer} = call;")));
    }
}

internal partial class MethodBuilder
{


    internal record DelegateInfo(string Name, string Type, string Parameters);

    internal record MethodInfo(string Name, string ReturnType, string ReturnString);
}
