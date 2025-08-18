namespace SweetMock.Builders;

using Generation;
using MemberBuilders;
using Utils;

internal class BaseClassBuilder(MockContext context)
{
    internal CodeBuilder BuildMockClass(CodeBuilder namespaceScope)
    {
        namespaceScope.Documentation(doc => doc
            .Summary($"Mock implementation of {context.Source.ToSeeCRef()}.", "Should only be used for testing purposes."));

        // TODO : Fix this uglyness
        var className = context.Source.ToString().Substring(context.Source.ContainingNamespace.ToString().Length + 1);

        namespaceScope.AddGeneratedCodeAttrib();
        namespaceScope.Scope($"internal partial class {context.MockType} : {className}{context.Constraints}", classScope =>
        {
            this.InitializeConfig(classScope);
            classScope.InitializeLogging();
            this.BuildMembers(classScope);
        });

        return namespaceScope;
    }

    private void InitializeConfig(CodeBuilder result) =>
        result.Region("Configuration", builder =>
        {
            builder
                .Documentation(doc => doc
                    .Summary("Configuration class for the mock."))
                .AddToConfig(context, config =>
                {
                    config.Add($"private readonly {context.MockType} target;");

                    config
                        .Documentation(doc => doc
                            .Summary($"Initializes a new instance of the <see cref=\"global::{context.Source.ToCRef()}.{context.ConfigName}\">{context.ConfigName}</see> class")
                            .Parameter("target", "The target mock class.")
                            .Parameter($"{context.ConfigName}", "Optional configuration method."))
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
