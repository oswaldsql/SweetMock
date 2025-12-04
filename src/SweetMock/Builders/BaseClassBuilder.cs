namespace SweetMock.Builders;

using Generation;
using MemberBuilders;
using Utils;

internal static class BaseClassBuilderExt
{
    internal static CodeBuilder BuildBaseClass(this CodeBuilder builder, MockContext context) =>
        new BaseClassBuilder(context).BuildMockClass(builder);
}

internal class BaseClassBuilder(MockContext context)
{
    private readonly INamedTypeSymbol source = context.Source;

    internal CodeBuilder BuildMockClass(CodeBuilder namespaceScope)
    {
        var className = this.source.ToDisplayString(Format.NameAndGenerics);

        return namespaceScope
            .Documentation($"Mock implementation of {this.source.ToSeeCRef()}.", "Should only be used for testing purposes.")
            .AddGeneratedCodeAttrib()
            .Scope($"internal partial class {context.MockType} : {className}{context.Constraints}", classScope =>
            {
                classScope.Add($"private const string _containerName = \"{this.source.ToDisplayString(Format.ExtendedTypeFormat)}\";").BR();
                this.InitializeConfig(classScope);
                classScope.InitializeLogging(context);
                this.BuildMembers(classScope);
            })
            .BR();
    }

    private void InitializeConfig(CodeBuilder result) =>
        result.Region("Configuration", builder =>
        {
            builder
                .Documentation("Configuration class for the mock.")
                .AddToConfig(context, config =>
                {
                    config.Add($"private readonly {context.MockType} target;");

                    config
                        .Documentation(doc => doc
                            .Summary($"Initializes the configuration for {this.source.ToSeeCRef()} instance of the {context.ConfigName} class")
                            .Parameter("target", "The target mock class.")
                            .Parameter("config", "Optional configuration method."))
                        .Scope($"public {context.ConfigName}({context.MockType} target, System.Action<{context.ConfigName}>? config = null)", methodScope => methodScope
                            .Add("this.target = target;")
                            .Add("config?.Invoke(this);"));
                });
        });

    private void BuildMembers(CodeBuilder classScope)
    {
        var candidates = context.GetCandidates().Distinct(SymbolEqualityComparer.Default).ToArray();

        var constructors = candidates.OfType<IMethodSymbol>().Where(t => t.MethodKind == MethodKind.Constructor);
        ConstructorBuilder.Render(classScope, context, constructors);

        var methods = candidates.OfType<IMethodSymbol>().Where(t => t.MethodKind == MethodKind.Ordinary);
        MethodBuilder.Render(classScope, context, methods);

        var properties = candidates.OfType<IPropertySymbol>().Where(t => !t.IsIndexer);
        PropertyBuilder.Render(classScope, context, properties);

        var indexers = candidates.OfType<IPropertySymbol>().Where(t => t.IsIndexer);
        IndexBuilder.Render(classScope, context, indexers);

        var events = candidates.OfType<IEventSymbol>();
        EventBuilder.Render(classScope, context, events);
    }
}
