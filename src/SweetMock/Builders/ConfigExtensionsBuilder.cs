namespace SweetMock.Builders;

using System.Linq;
using MemberBuilders;
using Microsoft.CodeAnalysis;
using Utils;

public class ConfigExtensionsBuilder
{
    public string Build(MockDetails mock)
    {
        var builder = new CodeBuilder();

        builder.AddFileHeader();

        builder.Add($"""
                     #nullable enable
                     using System.Linq;

                     using System;
                     """);

        builder.Scope($"namespace {mock.Namespace}", namespaceScope =>
            {
                namespaceScope.AddGeneratedCodeAttrib()
                    .Scope($"internal static class {mock.MockName}_ConfigExtensions", classScope =>
                    {
                        builder.Add(this.BuildMembers(mock));
                    });
            }
        );

        return builder.ToString();
    }

    private CodeBuilder BuildMembers(MockDetails mock)
    {
        using CodeBuilder result = new();

        var candidates = mock.GetCandidates();

//        var constructors = candidates.OfType<IMethodSymbol>().Where(t => t.MethodKind == MethodKind.Constructor);
//        result.Add(ConstructorBuilder.BuildConfigExtensions(details, constructors));
//
//        var methods = candidates.OfType<IMethodSymbol>().Where(t => t.MethodKind == MethodKind.Ordinary);
//        result.Add(MethodBuilder.BuildConfigExtensions(details, methods));

        var properties = candidates.OfType<IPropertySymbol>().Where(t => !t.IsIndexer);
        result.Add(PropertyBuilder.BuildConfigExtensions(mock, properties));

        var indexers = candidates.OfType<IPropertySymbol>().Where(t => t.IsIndexer);
        result.Add(IndexBuilder.BuildConfigExtensions(mock, indexers));

        var events = candidates.OfType<IEventSymbol>();
        result.Add(EventBuilder.BuildConfigExtensions(mock, events));

        return result;
    }
}
