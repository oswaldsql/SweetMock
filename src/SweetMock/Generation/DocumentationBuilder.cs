// ReSharper disable EnforceIfStatementBraces
// ReSharper disable HeuristicUnreachableCode

namespace SweetMock.Generation;

internal class DocumentationBuilder(CodeBuilder builder) : CodeBuilder(builder)
{
    public DocumentationBuilder Summary(params string[] summaries)
    {
        this.Add("/// <summary>")
            .Add(summaries, summary => $"///    {summary}")
            .Add("/// </summary>");

        return this;
    }

    public DocumentationBuilder Parameter(string name, string description, bool condition = true)
    {
        if (condition)
        {
            this.Add($"/// <param name=\"{name}\">{description}</param>");
        }

        return this;
    }

    public DocumentationBuilder Parameter(IEnumerable<IParameterSymbol> source, Func<IParameterSymbol, string> description) =>
        this.Parameter(source, t => t.Name, description);

    public DocumentationBuilder Parameter<T>(IEnumerable<T> source, Func<T, string> name, Func<T, string> description)
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
