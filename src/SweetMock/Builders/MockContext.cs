namespace SweetMock.Builders;

using Utils;

public class MockContext
{
    public MockContext(INamedTypeSymbol source)
    {
        this.Source = source;

        var generics = source.GetTypeGenerics();

        this.MockType = $"MockOf_{source.Name}{generics}";
        this.MockName = "MockOf_" + source.Name;
        this.Constraints = source.ToConstraints();
        this.ConfigName = "MockConfig";
    }

    public INamedTypeSymbol Source { get; }
    public string MockType { get; }
    public string MockName { get; }
    public string Constraints { get; }
    public string ConfigName { get; }

    internal ISymbol[] GetCandidates()
    {
        var allMembers = this.Source.GetMembers().ToList();
        AddInheritedInterfaces(allMembers, this.Source);
        var m = allMembers.Where(IsCandidate).ToArray();
        return m;
    }

    internal static void AddInheritedInterfaces(List<ISymbol> memberCandidates, INamedTypeSymbol namedTypeSymbol)
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
        if (symbol is not (IMethodSymbol or IPropertySymbol or IEventSymbol)) return false;
        if (symbol.IsSealed || symbol.IsStatic) return false;
        if (symbol is IMethodSymbol { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet or MethodKind.EventAdd or MethodKind.EventRaise or MethodKind.EventRemove }) return false;
        if (symbol.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Protected or Accessibility.Internal or Accessibility.ProtectedOrInternal or Accessibility.ProtectedAndInternal)) return false;
        if (symbol is IMethodSymbol { MethodKind: MethodKind.Constructor }) return true;
        if (symbol.IsAbstract || symbol.IsVirtual || symbol.IsOverride) return true;

        return false;
    }
}
