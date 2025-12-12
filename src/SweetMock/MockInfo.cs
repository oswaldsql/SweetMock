namespace SweetMock;

using Builders;
using Utils;

public class MockInfo
{
    private MockInfo(INamedTypeSymbol source, string mockClass, MockKind kind, string contextConfigName, INamedTypeSymbol? implementation = null)
    {
        this.MockType = $"MockOf_{source.Name}{source.GetTypeGenerics()}";
        this.MockName = "MockOf_" + source.Name;
        this.Constraints = source.ToConstraints();
        this.Source = source;
        this.MockClass = mockClass;
        this.Kind = kind;
        this.ContextConfigName = contextConfigName;
        this.Implementation = implementation;

        this.Candidates = this.GetCandidates();

        this.Name = source.Name;
    }

    public ISymbol[] Candidates { get; set; }

    public string MockType { get; }
    public string MockName { get; }
    public string Constraints { get; }
    public string ConfigName { get; } = "MockConfig";

    public string Name { get; init; }

    public INamedTypeSymbol Source { get; init; }
    public string MockClass { get; init; }
    public MockKind Kind { get; init; }
    public string ContextConfigName { get; init; }
    public INamedTypeSymbol? Implementation { get; init; }

    private ISymbol[] GetCandidates()
    {
        var allMembers = this.Source.GetMembers().ToList();
        AddInheritedInterfaces(allMembers, this.Source);
        var m = allMembers.Where(IsCandidate).ToArray();
        return m;
    }

    private static void AddInheritedInterfaces(List<ISymbol> memberCandidates, INamedTypeSymbol namedTypeSymbol)
    {
        if (namedTypeSymbol.TypeKind != TypeKind.Interface)
        {
            return;
        }

        var allInterfaces = namedTypeSymbol.AllInterfaces;
        foreach (var inherited in allInterfaces)
        {
            memberCandidates.AddRange(inherited.GetMembers());
            AddInheritedInterfaces(memberCandidates, inherited);
        }
    }

    private static bool IsCandidate(ISymbol symbol)
    {
        if (symbol is not (IMethodSymbol or IPropertySymbol or IEventSymbol))
        {
            return false;
        }

        if (symbol.IsSealed || symbol.IsStatic)
        {
            return false;
        }

        if (symbol is IMethodSymbol { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet or MethodKind.EventAdd or MethodKind.EventRaise or MethodKind.EventRemove })
        {
            return false;
        }

        if (symbol.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Protected or Accessibility.Internal or Accessibility.ProtectedOrInternal or Accessibility.ProtectedAndInternal))
        {
            return false;
        }

        if (symbol is IMethodSymbol { MethodKind: MethodKind.Constructor })
        {
            return true;
        }

        if (symbol.IsAbstract || symbol.IsVirtual || symbol.IsOverride)
        {
            return true;
        }

        return false;
    }

    public static MockInfo Generated(MockContext context) =>
        new(context.Source, context.Source.ContainingNamespace + "." + context.MockName, MockKind.Generated, context.ConfigName);

    public static MockInfo Wrapped(INamedTypeSymbol mockType, INamedTypeSymbol implementation) =>
        new(mockType, implementation.ContainingNamespace + "." + implementation.ToDisplayString(Format.Format2), MockKind.Wrapper, "MockConfig", implementation);

    public static MockInfo BuildIn(INamedTypeSymbol symbol) =>
        new(symbol, symbol.ContainingNamespace + ".MockOf_" + symbol.Name, MockKind.BuildIn, "MockConfig");
}

public enum MockKind
{
    Generated,
    Wrapper,
    Direct,
    BuildIn
}
