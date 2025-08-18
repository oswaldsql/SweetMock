namespace SweetMock.Builders;

public static class MockContextExtensions
{
    internal static ISymbol[] GetCandidates(this MockContext context)
    {
        var allMembers = context.Source.GetMembers().ToList();
        AddInheritedInterfaces(allMembers, context.Source);
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

    internal static bool IsReturningGenericTask(this IMethodSymbol method) =>
        method.ReturnType.ToString().StartsWith("System.Threading.Tasks.Task<") &&
        ((INamedTypeSymbol)method.ReturnType).TypeArguments.Length > 0;

    internal static bool IsReturningGenericValueTask(this IMethodSymbol method) =>
        method.ReturnType.ToString().StartsWith("System.Threading.Tasks.ValueTask<") &&
        ((INamedTypeSymbol)method.ReturnType).TypeArguments.Length > 0;

    internal static bool IsGenericTask(this ITypeSymbol type) =>
        type.ToString().StartsWith("System.Threading.Tasks.Task<") &&
        ((INamedTypeSymbol)type).TypeArguments.Length > 0;

    internal static bool IsGenericValueTask(this ITypeSymbol type) =>
        type.ToString().StartsWith("System.Threading.Tasks.ValueTask<") &&
        ((INamedTypeSymbol)type).TypeArguments.Length > 0;
}
