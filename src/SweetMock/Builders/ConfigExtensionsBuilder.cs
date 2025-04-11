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
                        classScope.Add(BuildMembers(mock));
                    });
            }
        );

        return builder.ToString();
    }

    private static CodeBuilder BuildMembers(MockDetails mock)
    {
        CodeBuilder result = new();

        var candidates = mock.GetCandidates();

//        var constructors = candidates.OfType<IMethodSymbol>().Where(t => t.MethodKind == MethodKind.Constructor);
//        result.Add(ConstructorBuilder.BuildConfigExtensions(details, constructors));

        var methods = candidates.OfType<IMethodSymbol>().Where(t => t.MethodKind == MethodKind.Ordinary);
        result.Add(MethodBuilder.BuildConfigExtensions(mock, methods));

        var properties = candidates.OfType<IPropertySymbol>().Where(t => !t.IsIndexer);
        result.Add(PropertyBuilder.BuildConfigExtensions(mock, properties));

        var indexers = candidates.OfType<IPropertySymbol>().Where(t => t.IsIndexer);
        result.Add(IndexBuilder.BuildConfigExtensions(mock, indexers));

        var events = candidates.OfType<IEventSymbol>();
        result.Add(EventBuilder.BuildConfigExtensions(mock, events));

        return result;
    }

    internal static CodeBuilder AddConfigExtension(this CodeBuilder result, MockDetails mock, ISymbol symbol, string[] arguments, Action<CodeBuilder> build)
    {
        var name = symbol.Name;
        if (name == "this[]")
        {
            name = "Indexer";
        }

        var constraints = mock.Target.TypeArguments.ToConstraints();

        if (mock.Target.TypeArguments.Length != 0)
        {
            name = name + "<" + string.Join(", ", mock.Target.TypeArguments.Select(t => t.Name)) + ">";
            //result.Add(ConstraintBuilder.ToConstraints(mock.Target.TypeArguments));
//            foreach (var typeArgument in mock.Target.TypeArguments)
//            {
//                result.Add("//" + typeArgument.Name + " : " + ConstraintBuilder.ToConstraints([typeArgument]));
//            }
        }

        if (symbol is IMethodSymbol namedTypeSymbol && namedTypeSymbol.TypeArguments.Length != 0)
        {
            foreach (var typeArgument in namedTypeSymbol.TypeArguments)
            {
                result.Add("//" + typeArgument.Name + " : " + ConstraintBuilder.ToConstraints([typeArgument]));
            }

        }

        var args = "";
        if (arguments.Length > 0)
        {
            args = ", " + string.Join(" , ", arguments);
        }

        result.Add($"public static {mock.MockType}.Config {name}(this {mock.MockType}.Config config{args})" + constraints);
        result.Add("{").Indent();
        build(result);
        result.Add("return config;");
        result.Unindent().Add("}");
        return result;
    }
}
