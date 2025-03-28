namespace SweetMock.Builders;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

public static class MockDetailsExtensions
{
    public static ILookup<string, ISymbol> GetMembers(this MockDetails details)
    {
        var accessibilityFilter = new Func<Accessibility, bool>(accessibility => accessibility == Accessibility.Public || accessibility == Accessibility.Protected);

        var memberCandidates = details.Target.GetMembers().Where(t => accessibilityFilter(t.DeclaredAccessibility)).ToList();

        if (details.Target.TypeKind == TypeKind.Interface) AddInheritedInterfaces(memberCandidates, details.Target);

        return memberCandidates.Distinct(SymbolEqualityComparer.IncludeNullability).ToLookup(t => t.Name);
    }

    internal static void AddInheritedInterfaces(List<ISymbol> memberCandidates, INamedTypeSymbol namedTypeSymbol)
    {
        var allInterfaces = namedTypeSymbol.AllInterfaces;
        foreach (var inherited in allInterfaces)
        {
            memberCandidates.AddRange(inherited.GetMembers());
            AddInheritedInterfaces(memberCandidates, inherited);
        }
    }
}
