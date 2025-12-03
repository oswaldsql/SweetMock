namespace SweetMock.Builders.MemberBuilders;

using Generation;
using Utils;

/// <summary>
/// Represents a builder for constructing mock constructors.
/// </summary>
internal class ConstructorBuilder(MockContext context) {
    public static void Render(CodeBuilder classScope, MockContext context, IEnumerable<IMethodSymbol> constructors)
    {
        var builder = new ConstructorBuilder(context);
        builder.Build(classScope, constructors);
    }

    private void Build(CodeBuilder classScope, IEnumerable<IMethodSymbol> constructors)
    {
        var distinctConstructors = constructors.Distinct(SymbolEqualityComparer.Default).OfType<IMethodSymbol>().ToArray();

        classScope.Region("Constructors", builder =>
        {
            builder
                .Add("private global::SweetMock.MockOptions? _sweetMockOptions {get;set;}")
                .Add("private string _sweetMockInstanceName {get; set;} = \"\";")
                .BR();

            this.CreateLogArgumentsRecord(distinctConstructors, builder);

            if (distinctConstructors.Length != 0)
            {
                this.BuildConstructors(builder, distinctConstructors);
            }
            else
            {
                this.BuildEmptyConstructor(builder);
            }
        });
    }

    private void CreateLogArgumentsRecord(IMethodSymbol[] distinctConstructors, CodeBuilder builder)
    {
        var arguments = distinctConstructors.SelectMany(t => t.Parameters).ToLookup(t => t.Name);
        var args = string.Join(", ", arguments.Select(GetArgString));

        builder
            .Add($"public record {context.Source.Name}_Arguments(")
            .Indent(scope => scope
                .Add("global::System.String? InstanceName,")
                .Add("global::System.String MethodSignature" + (arguments.Count != 0 ? "," : ""))
                .Add(args))
            .Add($") : ArgumentBase(_containerName, \"{context.Source.Name}\", MethodSignature, InstanceName);")
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

    private void BuildConstructors(CodeBuilder builder, IEnumerable<IMethodSymbol> constructors)
    {
        foreach (var constructor in constructors)
        {
            builder.Scope(this.ConstructorSignature(constructor), ctor => ctor
                .Add("this._sweetMockOptions = options ?? global::SweetMock.MockOptions.Default;")
                .Add("this._sweetMockCallLog = this._sweetMockOptions.Logger ?? this._sweetMockCallLog;")
                .Add($"this._sweetMockInstanceName = this._sweetMockOptions.InstanceName ?? \"{context.Source.Name}\";")
                .Add($"this._log(new {context.Source.Name}_Arguments(this._sweetMockInstanceName, \"{context.Source.Name}\"{string.Join("", constructor.Parameters.Where(t => t.RefKind == RefKind.None).Select(t => $", {t.Name} : {t.Name}"))}));")
                .Add($"new {context.ConfigName}(this, config);")
            );
        }
    }

    private string ConstructorSignature(IMethodSymbol constructor)
    {
        var parameterList = constructor.Parameters.ToString(p => $"{p.Type} {p.Name}, ", "");
        var baseArguments = constructor.Parameters.ToString(p => p.Name);

        return $"protected internal MockOf_{context.Source.Name}({parameterList}System.Action<{context.ConfigName}>? config = null, global::SweetMock.MockOptions? options = null) : base({baseArguments})";
    }

    private void BuildEmptyConstructor(CodeBuilder builder) =>
        builder
            .Scope($"internal protected MockOf_{context.Source.Name}(System.Action<{context.ConfigName}>? config = null, global::SweetMock.MockOptions? options = null)", methodScope => methodScope
                .Add("this._sweetMockOptions = options ?? global::SweetMock.MockOptions.Default;")
                .Add("this._sweetMockCallLog = options?.Logger ?? _sweetMockCallLog;")
                .Add($"this._sweetMockInstanceName = options?.InstanceName ?? \"{context.Source.Name}\";")
                .Add($"this._log(new {context.Source.Name}_Arguments(_sweetMockInstanceName, \"{context.Source.Name}\"));")
                .Add($"new {context.ConfigName}(this, config);"));
}
