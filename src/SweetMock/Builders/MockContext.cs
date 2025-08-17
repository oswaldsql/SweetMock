namespace SweetMock.Builders;

public class MockContext
{
    public MockContext(INamedTypeSymbol source)
    {
        this.Source = source;
        var mockType = "MockOf_" + source.Name;
        var mockName = "MockOf_" + source.Name;
        var constraints = "";

        var typeArguments = source.TypeArguments;
        if (typeArguments.Length > 0)
        {
            var generics = string.Join(", ", typeArguments.Select(t => t.Name));
            mockType = $"MockOf_{source.Name}<{generics}>";
            constraints = typeArguments.ToConstraints();
        }

        this.MockType = mockType;
        this.MockName = mockName;
        this.Constraints = constraints;
        this.ConfigName = "MockConfig";
    }

    public INamedTypeSymbol Source { get; init; }
    public string MockType { get; init; }
    public string MockName { get; init; }
    public string Constraints { get; init; }
    public string ConfigName { get; init; }
}
