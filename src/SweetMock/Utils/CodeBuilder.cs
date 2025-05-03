namespace SweetMock.Utils;

using System;
using System.Linq;
using System.Text;

internal class CodeBuilder
{
    private readonly StringBuilder result = new();

    private const int MaxIndent = 50;
    private const int IndentSize = 4;
    private static readonly string[] IndentCache;

    static CodeBuilder() =>
        IndentCache = Enumerable.Range(0, MaxIndent)
            .Select(i => new string(' ', i * IndentSize))
            .ToArray();

    public int Indentation { get; private set; }

    private string GetIndentation =>
        this.Indentation < MaxIndent
            ? IndentCache[this.Indentation]
            : new(' ', this.Indentation * 4);

    public CodeBuilder Indent()
    {
        this.Indentation += 1;
        return this;
    }

    public CodeBuilder Unindent()
    {
        this.Indentation -= 1;

        if (this.Indentation < 0)
        {
            throw new("Indentation can not be less than 0");
        }

        return this;
    }

    public CodeBuilder AddLineBreak()
    {
        this.result.AppendLine();
        return this;
    }

    public CodeBuilder AddLines(string text)
    {
        var strings = text.Split(["\r\n", "\n"], StringSplitOptions.None);
        foreach (var s in strings)
        {
            switch (s)
            {
                case "->":
                    this.Indentation += 1;
                    continue;
                case "<-":
                    this.Indentation -= 1;
                    continue;
                case "--":
                    continue;
                default:
                    this.result.Append(this.GetIndentation).AppendLine(s);
                    break;
            }
        }

        return this;
    }

    public CodeBuilder Add(string line)
    {
        this.result.Append(this.GetIndentation).AppendLine(line);
        return this;
    }

    public CodeBuilder Add(bool condition, Func<string> add) => condition ? this.Add(add()) : this;

    public CodeBuilder Condition(bool condition, Action<CodeBuilder> add)
    {
        if (condition)
        {
            add(this);
        }

        return this;
    }

    public override string ToString() => string.Join("\r\n", this.result);

    public SurroundingCodeBuilder Region(string region) => new(this, "#region " + region, "#endregion");

    public CodeBuilder Scope(string prefix, Action<CodeBuilder> body)
    {
        this.Add(prefix + "{").Indent();

        body(this);

        this.Unindent().Add("}");
        return this;
    }
}
