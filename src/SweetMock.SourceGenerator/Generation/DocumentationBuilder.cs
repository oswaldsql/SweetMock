// ReSharper disable EnforceIfStatementBraces
// ReSharper disable HeuristicUnreachableCode

namespace SweetMock.Generation;

internal class DocumentationBuilder(CodeBuilder builder) : CodeBuilder(builder)
{
    public DocumentationBuilder Summary(params string[] summaries)
    {
        this.Add("/// <summary>")
            .AddMultiple(summaries, summary => $"///    {summary}")
            .Add("/// </summary>");

        return this;
    }

    public DocumentationBuilder Parameter(string name, string description)
    {
        this.Add($"/// <param name=\"{name}\">{description}</param>");

        return this;
    }

    public DocumentationBuilder ParameterIf(bool condition, string name, string description)
    {
        if (condition)
        {
            this.Parameter(name, description);
        }

        return this;
    }

    public DocumentationBuilder Parameters(IEnumerable<IParameterSymbol> source, Func<IParameterSymbol, string> description) =>
        this.Parameters(source, t => t.Name, description);

    public DocumentationBuilder Parameters<T>(IEnumerable<T> source, Func<T, string> name, Func<T, string> description)
    {
        foreach (var s in source)
        {
            this.Parameter(name(s), description(s));
        }

        return this;
    }

    public void Returns(string description) =>
        this.Add($"/// <returns>{description}</returns>");
}
