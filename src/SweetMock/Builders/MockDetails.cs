namespace SweetMock.Builders;

public record MockDetails(INamedTypeSymbol Target, string Namespace, string SourceName, string MockType, string MockName, string Constraints);