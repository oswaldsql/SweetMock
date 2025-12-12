namespace SweetMock.Builders;

using Utils;

public partial class FixtureBuilder
{
    public class FixtureMetadata(INamedTypeSymbol fixtureSymbol, AttributeData[] attributes)
    {
        public AttributeData[] Attributes { get; } = attributes;

        public FixtureTargetCtorMetadata TargetCtor { get; } = new(fixtureSymbol.Constructors.FirstOrDefault(t => t.DeclaredAccessibility is not Accessibility.Private and not Accessibility.Protected && t.Parameters.Length > 0));

        public string Name { get; } = fixtureSymbol.Name;

        public string Generics { get; } = fixtureSymbol.GetTypeGenerics();

        public string Constraints { get; } = fixtureSymbol.ToConstraints();

        public string Namespace { get; } = fixtureSymbol.ContainingNamespace.ToDisplayString();

        public string ToSeeCRef { get; } = fixtureSymbol.ToSeeCRef();

        public string TypeString { get; } = fixtureSymbol.ToDisplayString(Format.ToFullNameFormatWithGlobal);

        public IParameterSymbol[] Parameters => this.TargetCtor.Parameters;

        public string FileName { get; } = fixtureSymbol.ToCRef().Replace("<", "_").Replace(">", "").Replace(", ", "_") + ".Fixture.g.cs";
    }

    public class FixtureTargetCtorMetadata(IMethodSymbol? targetCtor)
    {
        public readonly IParameterSymbol[] Parameters = targetCtor?.Parameters.ToArray() ?? [];
    }
}
