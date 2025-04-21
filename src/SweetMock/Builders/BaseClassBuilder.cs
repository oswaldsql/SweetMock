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
            namespaceScope.AddSummary($"Mock implementation of <see cref=\"{details.Target.ToCRef()}\"/>.", "Should only be used for testing purposes.");
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
        using (result.Region("Configuration"))
        {
            result.AddSummary("Configuration class for the mock.");
            using (result.AddToConfig())
            {
                result.AddLines($"private readonly {details.MockType} target;");
                result.AddSummary($"Initializes a new instance of the <see cref=\"T:{details.Namespace}.{details.MockType}.Config\"/> class");
                result.AddParameter("target", "The target mock class.");
                result.AddParameter("config", "Optional configuration method.");
                result.AddLines($$"""
                              public Config({{details.MockType}} target, System.Action<Config>? config = null)
                              {
                                  this.target = target;
                                  config?.Invoke(this);
                              }
                             """);
            }
        }
    }

    private static void BuildMembers(CodeBuilder classScope, MockDetails details)
    {
        var candidates = details.GetCandidates();

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
