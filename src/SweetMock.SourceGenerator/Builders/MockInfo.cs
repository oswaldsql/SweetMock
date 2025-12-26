namespace SweetMock.Builders;

using Generation;
using Utils;

internal class MockInfo(INamedTypeSymbol source, string mockClass, MockKind kind, string contextConfigName, INamedTypeSymbol? implementation = null)
{
    public INamedTypeSymbol Source { get; } = source;

    public ISymbol[] Candidates { get; } = GetCandidates(source);

    public string MockType { get; } = $"MockOf_{source.Name}{source.GetTypeGenerics()}";

    public string MockName { get; } = "MockOf_" + source.Name;

    public string Constraints { get; } = source.ToConstraints();

    public string ConfigName { get; } = "MockConfig";

    public string Name { get; } = source.Name;

    public string FullName { get;  } = source.ToDisplayString(Format.ToFullNameFormatWithGlobal);

    public string Namespace { get; } = source.ContainingNamespace.ToDisplayString();

    public string MockClass { get; } = mockClass;

    public MockKind Kind { get;  } = kind;

    public string ContextConfigName { get;  } = contextConfigName;

    public INamedTypeSymbol? Implementation { get;  } = implementation;

    public string ToSeeCRef { get; } = source.ToSeeCRef();

    public string NameAndGenerics { get; } = source.ToDisplayString(Format.NameAndGenerics);

    public string ExtendedTypeFormat { get; } = source.ToDisplayString(Format.ExtendedTypeFormat);

    public string Generics { get; } = source.GetTypeGenerics();

    public string FullNameFormatWithoutGeneric { get; } = source.ToDisplayString(Format.ToFullNameFormatWithoutGeneric);

    private static ISymbol[] GetCandidates(INamedTypeSymbol source)
    {
        var allMembers = source.GetMembers().ToList();

        AddInheritedInterfaces(allMembers, source);

        return allMembers
            .Where(IsCandidate)
            .Distinct(SymbolEqualityComparer.Default)
            .ToArray();
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

    public static MockInfo Generated(INamedTypeSymbol source) =>
        new(source, source.ContainingNamespace + ".MockOf_" + source.Name, MockKind.Generated, "MockConfig");

//    public static MockInfo Generated(MockContext context) =>
//        Generated(context.Source);

    public static MockInfo Wrapped(INamedTypeSymbol mockType, INamedTypeSymbol implementation) =>
        new(mockType, implementation.ContainingNamespace + "." + implementation.ToDisplayString(Format.Format2), MockKind.Wrapper, "MockConfig", implementation);

    public static MockInfo BuildIn(INamedTypeSymbol symbol) =>
        new(symbol, symbol.ContainingNamespace + ".MockOf_" + symbol.Name, MockKind.BuildIn, "MockConfig");
}

internal enum MockKind
{
    Generated,
    Wrapper,
    Direct,
    BuildIn
}
