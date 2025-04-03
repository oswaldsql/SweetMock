namespace SweetMock.Builders;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

public static class MockDetailsExtensions
{
    internal static ISymbol[] GetCandidates(this MockDetails details)
    {
        var allMembers = details.Target.GetMembers().ToList();
        AddInheritedInterfaces(allMembers, details.Target);
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
        if (symbol.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Protected)) return false;
        if (symbol is IMethodSymbol { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet or MethodKind.EventAdd or MethodKind.EventRaise or MethodKind.EventRemove }) return false;
        if (symbol is IMethodSymbol { MethodKind: MethodKind.Constructor }) return true;
        if (symbol.IsAbstract || symbol.IsVirtual) return true;

        return false;
    }
}
