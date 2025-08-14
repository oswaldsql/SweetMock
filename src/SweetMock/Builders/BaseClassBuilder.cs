namespace SweetMock.Builders;

using Generation;
using MemberBuilders;
using Utils;

internal static class BaseClassBuilder
{
    internal static CodeBuilder BuildMockClass(this CodeBuilder namespaceScope, MockDetails details)
    {
        namespaceScope.Documentation(doc => doc
            .Summary($"Mock implementation of {details.Target.ToSeeCRef()}.", "Should only be used for testing purposes."));

        // TODO : Fix this uglyness
        var className = details.Target.ToString().Substring(details.Target.ContainingNamespace.ToString().Length + 1);

        namespaceScope.AddGeneratedCodeAttrib();
        namespaceScope.Scope($"internal partial class {details.MockType} : {className}{details.Constraints}", classScope =>
        {
            classScope.InitializeConfig(details);
            classScope.InitializeLogging();
            BuildMembers(classScope, details);
        });

        return namespaceScope;
    }

    private static void InitializeConfig(this CodeBuilder result, MockDetails details) =>
        result.Region("Configuration", builder =>
        {
            builder
                .Documentation(doc => doc
                    .Summary("Configuration class for the mock."))
                .AddToConfig(config =>
                {
                    config.Add($"private readonly {details.MockType} target;");

                    config.Documentation(doc => doc
                        .Summary($"Initializes a new instance of the <see cref=\"global::{details.Target.ToCRef()}.Config\"/> class")
                        .Parameter("target", "The target mock class.")
                        .Parameter("config", "Optional configuration method."));

                    config.Scope($"public Config({details.MockType} target, System.Action<Config>? config = null)", methodScope => methodScope
                        .Add("this.target = target;")
                        .Add("config?.Invoke(this);"));
                });
        });

    private static void BuildMembers(CodeBuilder classScope, MockDetails details)
    {
        var candidates = details.GetCandidates().Distinct(SymbolEqualityComparer.Default).ToArray();

        var constructors = candidates.OfType<IMethodSymbol>().Where(t => t.MethodKind == MethodKind.Constructor);
        ConstructorBuilder.Build(classScope, details, constructors);

        var methods = candidates.OfType<IMethodSymbol>().Where(t => t.MethodKind == MethodKind.Ordinary);
        new MethodBuilder(methods).Render(classScope);
        //MethodBuilder.Build(classScope, methods);

        var properties = candidates.OfType<IPropertySymbol>().Where(t => !t.IsIndexer);
        PropertyBuilder.Build(classScope, properties);

        var indexers = candidates.OfType<IPropertySymbol>().Where(t => t.IsIndexer);
        IndexBuilder.Build(classScope, indexers);

        var events = candidates.OfType<IEventSymbol>();
        EventBuilder.Build(classScope, events);
    }
}
