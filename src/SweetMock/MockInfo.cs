namespace SweetMock;

public record MockInfo(INamedTypeSymbol Source, string MockClass, MockKind Kind, string ContextConfigName, INamedTypeSymbol? Implementation = null);

public enum MockKind
{
    Generated,
    Wrapper,
    Direct,
    BuildIn
}
