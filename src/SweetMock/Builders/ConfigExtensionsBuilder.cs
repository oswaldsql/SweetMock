namespace SweetMock.Builders;

using System;
using System.Linq;
using MemberBuilders;
using Microsoft.CodeAnalysis;
using Utils;

public static class ConfigExtensionsBuilder
{
    public static string Build(MockDetails mock)
    {
        var builder = new CodeBuilder();

        builder.AddFileHeader();

        builder.AddLines($"""
                     #nullable enable
                     using System.Linq;

                     using System;
                     """);

        builder.Scope($"namespace {mock.Namespace}", namespaceScope =>
            {
                namespaceScope.AddGeneratedCodeAttrib()
                    .Scope($"internal static class {mock.MockName}_ConfigExtensions", classScope =>
                    {
                        BuildMembers(builder, mock);
                    });
            }
        );

        return builder.ToString();
    }

    private static void BuildMembers(CodeBuilder builder, MockDetails mock)
    {
        var candidates = mock.GetCandidates();

//        var constructors = candidates.OfType<IMethodSymbol>().Where(t => t.MethodKind == MethodKind.Constructor);
//        result.Add(ConstructorBuilder.BuildConfigExtensions(details, constructors));

        var methods = candidates.OfType<IMethodSymbol>().Where(t => t.MethodKind == MethodKind.Ordinary);
        MethodBuilder.BuildConfigExtensions(builder, mock, methods);

        var properties = candidates.OfType<IPropertySymbol>().Where(t => !t.IsIndexer);
        PropertyBuilder.BuildConfigExtensions(builder, mock, properties);

        var indexers = candidates.OfType<IPropertySymbol>().Where(t => t.IsIndexer);
        IndexBuilder.BuildConfigExtensions(builder, mock, indexers);

        var events = candidates.OfType<IEventSymbol>();
        EventBuilder.BuildConfigExtensions(builder, mock, events);
    }

    internal static CodeBuilder AddConfigExtension(this CodeBuilder result, MockDetails mock, ISymbol symbol, string[] arguments, Action<CodeBuilder> build)
    {
        var name = symbol.Name;
        if (name == "this[]")
        {
            name = "Indexer";
        }

        var constraints = mock.Target.TypeArguments.ToConstraints();

        if (symbol is IMethodSymbol method && method.TypeArguments.Length != 0)
        {
            name = name + "<" + string.Join(", ", method.TypeArguments.Select(t => t.Name)) + ">";
        }

        if (symbol is IMethodSymbol namedTypeSymbol && namedTypeSymbol.TypeArguments.Length != 0)
        {
            foreach (var typeArgument in namedTypeSymbol.TypeArguments)
            {
                result.AddLines("//" + typeArgument.Name + " : " + ConstraintBuilder.ToConstraints([typeArgument]));
            }

        }

        var args = "";
        if (arguments.Length > 0)
        {
            args = ", " + string.Join(" , ", arguments);
        }

        result.AddLines($"public static {mock.MockType}.Config {name}(this {mock.MockType}.Config config{args})" + constraints);
        result.AddLines("{").Indent();
        build(result);
        result.AddLines("return config;");
        result.Unindent().AddLines("}");
        return result;
    }
}
