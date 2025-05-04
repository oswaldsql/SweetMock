namespace SweetMock.Builders;

using System.Linq;
using MemberBuilders;
using Microsoft.CodeAnalysis;
using Utils;

internal static class BaseClassBuilder
{
    public static CodeBuilder Build(MockDetails details)
    {
        CodeBuilder fileScope = new();

        fileScope.AddFileHeader();
        fileScope.Add("#nullable enable");
        fileScope.Scope($"namespace {details.Namespace}", namespaceScope =>
        {
            namespaceScope.Documentation(doc => doc
                .Summary($"Mock implementation of <see cref=\"{details.Target.ToCRef()}\"/>.", "Should only be used for testing purposes."));

            namespaceScope.AddGeneratedCodeAttrib();
            namespaceScope.Scope($"internal partial class {details.MockType} : {details.SourceName}{details.Constraints}", classScope =>
            {
                classScope.InitializeConfig(details);
                classScope.InitializeLogging();
                BuildMembers(classScope, details);
            });
        });

        return fileScope;
    }

    private static void InitializeConfig(this CodeBuilder result, MockDetails details)
    {
        result.Region("Configuration", builder =>
        {
            builder.Documentation(doc => doc
                .Summary("Configuration class for the mock."));

            builder.AddToConfig(c =>
            {
                c.Add($"private readonly {details.MockType} target;");

                c.Documentation(doc => doc
                    .Summary($"Initializes a new instance of the <see cref=\"T:{details.Namespace}.{details.MockType}.Config\"/> class")
                    .Parameter("target", "The target mock class.")
                    .Parameter("config", "Optional configuration method."));

                c.Scope($"public Config({details.MockType} target, System.Action<Config>? config = null)", b => b
                    .Add("this.target = target;")
                    .Add("config?.Invoke(this);"));
            });
        });
    }

    private static void BuildMembers(CodeBuilder classScope, MockDetails details)
    {
        var candidates = details.GetCandidates().Distinct(SymbolEqualityComparer.Default).ToArray();

        var constructors = candidates.OfType<IMethodSymbol>().Where(t => t.MethodKind == MethodKind.Constructor);
        ConstructorBuilder.Build(classScope, details, constructors);

        var methods = candidates.OfType<IMethodSymbol>().Where(t => t.MethodKind == MethodKind.Ordinary);
        MethodBuilder.Build(classScope, methods);

        var properties = candidates.OfType<IPropertySymbol>().Where(t => !t.IsIndexer);
        PropertyBuilder.Build(classScope, properties);

        var indexers = candidates.OfType<IPropertySymbol>().Where(t => t.IsIndexer);
        IndexBuilder.Build(classScope, indexers);

        var events = candidates.OfType<IEventSymbol>();
        EventBuilder.Build(classScope, events);
    }
}
