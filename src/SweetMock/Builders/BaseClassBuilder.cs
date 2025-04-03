namespace SweetMock.Builders;

using System.Linq;
using MemberBuilders;
using Microsoft.CodeAnalysis;
using Utils;

internal static class BaseClassBuilder
{
    public static CodeBuilder Build(MockDetails details)
    {
        using CodeBuilder fileScope = new();

        var documentationName = details.Target.ToCRef();

        fileScope.AddFileHeader();
        fileScope.Add("#nullable enable");
        fileScope.Scope($"namespace {details.Namespace}", namespaceScope =>
        {
            namespaceScope.AddSummary($"Mock implementation of <see cref=\"{documentationName}\"/>.", "Should only be used for testing purposes.");
            namespaceScope.AddGeneratedCodeAttrib();
            namespaceScope.Scope($"internal class {details.MockType} : {details.SourceName}{details.Constraints}", classScope =>
            {
                namespaceScope.InitializeConfig(details);

                namespaceScope.InitializeLogging();

                namespaceScope.Add(BuildMembers(details));
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
                result.Add($"private readonly {details.MockType} target;");
                result.AddSummary($"Initializes a new instance of the <see cref=\"T:{details.Namespace}.{details.MockType}.Config\"/> class");
                result.AddParameter("target", "The target mock class.");
                result.AddParameter("config", "Optional configuration method.");
                result.Add($$"""
                              public Config({{details.MockType}} target, System.Action<Config>? config = null)
                              {
                                  this.target = target;
                                  config?.Invoke(this);
                              }
                             """);
            }
        }
    }

    private static CodeBuilder BuildMembers(MockDetails details)
    {
        using CodeBuilder result = new();

        var candidates = details.GetCandidates();

        var constructors = candidates.OfType<IMethodSymbol>().Where(t => t.MethodKind == MethodKind.Constructor);
        result.Add(ConstructorBuilder.Build(details, constructors));

        var methods = candidates.OfType<IMethodSymbol>().Where(t => t.MethodKind == MethodKind.Ordinary);
        result.Add(MethodBuilder.Build(methods));

        var properties = candidates.OfType<IPropertySymbol>().Where(t => !t.IsIndexer);
        result.Add(PropertyBuilder.Build(properties));

        var indexers = candidates.OfType<IPropertySymbol>().Where(t => t.IsIndexer);
        result.Add(IndexBuilder.Build(indexers));

        var events = candidates.OfType<IEventSymbol>();
        result.Add(EventBuilder.Build(events));

        return result;
    }
}
