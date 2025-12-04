namespace SweetMock.Builders;

using Utils;

public class MockContext(INamedTypeSymbol source)
{
    public INamedTypeSymbol Source { get; } = source;
    public string Name => Source.Name;
    public string MockType { get; } = $"MockOf_{source.Name}{source.GetTypeGenerics()}";
    public string MockName { get; } = "MockOf_" + source.Name;
    public string Constraints { get; } = source.ToConstraints();
    public string ConfigName { get; } = "MockConfig";

    internal ISymbol[] GetCandidates()
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
        if (symbol is not (IMethodSymbol or IPropertySymbol or IEventSymbol)) return false;
        if (symbol.IsSealed || symbol.IsStatic) return false;
        if (symbol is IMethodSymbol { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet or MethodKind.EventAdd or MethodKind.EventRaise or MethodKind.EventRemove }) return false;
        if (symbol.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Protected or Accessibility.Internal or Accessibility.ProtectedOrInternal or Accessibility.ProtectedAndInternal)) return false;
        if (symbol is IMethodSymbol { MethodKind: MethodKind.Constructor }) return true;
        if (symbol.IsAbstract || symbol.IsVirtual || symbol.IsOverride) return true;

        return false;
    }
}
