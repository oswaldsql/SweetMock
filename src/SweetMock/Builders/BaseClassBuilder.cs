namespace SweetMock.Builders;

using System;
using System.Collections.Generic;
using System.Linq;
using MemberBuilders;
using Microsoft.CodeAnalysis;
using Utils;

internal static class BaseClassBuilder
{
    public static CodeBuilder Build(MockDetails details)
    {
        using CodeBuilder result = new();

        var documentationName = details.Target.ToCRef();

        result.AddFileHeader();
        result.Add($$"""
                     #nullable enable
                     namespace {{details.Namespace}}
                     {
                     """).Indent();
        result.AddSummary($"Mock implementation of <see cref=\"{documentationName}\"/>. Should only be used for testing purposes.");
        result.Add($$"""
                     [System.CodeDom.Compiler.GeneratedCode("SweetMock","{{SourceGeneratorMetadata.Version}}")]
                     internal class {{details.MockType}} : {{details.SourceName}} {{details.Constraints}}
                     {
                     """).Indent();

        InitializeConfig(details, result);

        result.InitializeLogging();

        result.Add(BuildMembers(details));

        result.Add("""
                    <-
                    }
                    <-
                    }
                    """);

        return result;
    }

    private static void InitializeConfig(MockDetails details, CodeBuilder result)
    {
        using (result.Region("Configuration"))
        {
            result.AddSummary("Configuration class for the mock.");
            using (result.AddToConfig())
            {
                result.Add($"private readonly {details.MockType} target;");
                result.AddSummary("Initializes a new instance of the <see cref=\"T:Config\"/> class");
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

        var allMembers = details.Target.GetMembers().ToList();
        if(details.Target.TypeKind == TypeKind.Interface)
            AddInheritedInterfaces(allMembers, details.Target);
        var m = allMembers.Where(IsCandidate).ToArray();

        var constructors = m.OfType<IMethodSymbol>().Where(t => t.MethodKind == MethodKind.Constructor);
        result.Add(ConstructorBuilder.Build(details, constructors));

        m = m.Where(IsOverwritable).ToArray();

        var methods = m.OfType<IMethodSymbol>().Where(t => t.MethodKind == MethodKind.Ordinary);
        result.Add(MethodBuilder.Build(methods));

        var properties = m.OfType<IPropertySymbol>().Where(t => !t.IsIndexer);
        result.Add(PropertyBuilder.Build(properties));

        var indexers = m.OfType<IPropertySymbol>().Where(t => t.IsIndexer);
        result.Add(IndexBuilder.Build(indexers));

        var events = m.OfType<IEventSymbol>();
        result.Add(EventBuilder.Build(events));

        return result;
    }

    private static bool IsOverwritable(ISymbol t) => t.IsAbstract || t.IsVirtual;

    internal static void AddInheritedInterfaces(List<ISymbol> memberCandidates, INamedTypeSymbol namedTypeSymbol)
    {
        var allInterfaces = namedTypeSymbol.AllInterfaces;
        foreach (var inherited in allInterfaces)
        {
            memberCandidates.AddRange(inherited.GetMembers());
            AddInheritedInterfaces(memberCandidates, inherited);
        }
    }

    private static bool IsCandidate(ISymbol symbol) =>
        symbol is { IsStatic: false, IsSealed: false, DeclaredAccessibility: Accessibility.Public or Accessibility.Protected };
}
