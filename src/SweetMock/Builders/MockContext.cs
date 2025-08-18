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

    public INamedTypeSymbol Source { get; init; }
    public string MockType { get; init; }
    public string MockName { get; init; }
    public string Constraints { get; init; }
    public string ConfigName { get; init; }
}
