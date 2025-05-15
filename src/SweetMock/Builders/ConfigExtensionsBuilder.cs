namespace SweetMock.Builders;

using Generation;
using MemberBuilders;

public static class ConfigExtensionsBuilder
{
    public static string Build(MockDetails mock)
    {
        var result = new CodeBuilder();

        result.AddFileHeader();

        result
            .Add("#nullable enable")
            .Add("using System.Linq;")
            .Add("using System;")
            .AddLineBreak();

        result
            .Scope($"namespace {mock.Namespace}", namespaceScope => namespaceScope
                .Scope($"internal partial class {mock.MockType}", mockScope => mockScope
                    .Scope("internal partial class Config", configScope =>
                    {
                        BuildMembers(configScope, mock);
                    }))
            );

        return result.ToString();
    }

    private static void BuildMembers(CodeBuilder builder, MockDetails mock)
    {
        var candidates = mock.GetCandidates().Distinct(SymbolEqualityComparer.Default).ToArray();

        var methods = candidates.OfType<IMethodSymbol>().Where(t => t.MethodKind == MethodKind.Ordinary);
        MethodBuilder.BuildConfigExtensions(builder, mock, methods);

        var properties = candidates.OfType<IPropertySymbol>().Where(t => !t.IsIndexer);
        PropertyBuilder.BuildConfigExtensions(builder, mock, properties);

        var indexers = candidates.OfType<IPropertySymbol>().Where(t => t.IsIndexer);
        IndexBuilder.BuildConfigExtensions(builder, mock, indexers);

        var events = candidates.OfType<IEventSymbol>();
        EventBuilder.BuildConfigExtensions(builder, mock, events);
    }

    internal static void AddConfigExtension(this CodeBuilder result, MockDetails mock, ISymbol symbol, string[] arguments, Action<CodeBuilder> build)
    {
        var name = symbol.Name;
        if (name == "this[]")
        {
            name = "Indexer";
        }

        var args = string.Join(" , ", arguments);

        result.Add($"public Config {name}({args})")
            .Add("{").Indent()
            .Apply(build)
            .Add("return this;")
            .Unindent().Add("}");
    }
}
