namespace SweetMock.Generation;

using System.Text;

internal partial class CodeBuilder
{
    public CodeBuilder() { }

    public CodeBuilder(CodeBuilder innerBuilder)
    {
        this.result = innerBuilder.result;
        this.indentation = innerBuilder.indentation;
    }

    private readonly StringBuilder result = new();

    private const int MaxIndent = 50;
    private const int IndentSize = 4;
    private static readonly string[] IndentCache;
    private int indentation;

    static CodeBuilder() =>
        IndentCache = Enumerable.Range(0, MaxIndent)
            .Select(i => new string(' ', i * IndentSize))
            .ToArray();

    private string GetIndentation =>
        this.indentation < MaxIndent
            ? IndentCache[this.indentation]
            : new(' ', this.indentation * 4);

    public CodeBuilder Indent()
    {
        this.indentation += 1;
        return this;
    }

    public CodeBuilder Indent(Action<CodeBuilder> action) =>
        this.Indent()
            .Apply(action)
            .Unindent();


    public CodeBuilder Unindent()
    {
        this.indentation -= 1;

        return this.indentation < 0 ? throw new("Indentation can not be less than 0") : this;
    }

    public CodeBuilder Add(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return this;
        }
        this.result.Append(this.GetIndentation).AppendLine(line);
        return this;
    }

    public CodeBuilder Add<T>(IEnumerable<T> source, Func<T, string> format)
    {
        foreach (var s in source)
        {
            this.Add(format(s));
        }
        return this;
    }

    public CodeBuilder AddUnindented(string line)
    {
        this.result.AppendLine(line);
        return this;
    }

    public CodeBuilder BR()
    {
        this.result.AppendLine();
        return this;
    }

    public CodeBuilder AddIf(bool condition, Func<string> add) =>
        condition ? this.Add(add()) : this;

    public CodeBuilder AddIf(bool condition, Action<CodeBuilder> builder)
    {
        if (condition)
        {
            builder(this);
        }

        return this;
    }

    public override string ToString() => this.result.ToString();
}
