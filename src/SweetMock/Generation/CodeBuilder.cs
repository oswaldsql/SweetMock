namespace SweetMock.Generation;

using System;
using System.Linq;
using System.Text;

internal class CodeBuilder
{
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

    public CodeBuilder Unindent()
    {
        this.indentation -= 1;

        if (this.indentation < 0)
        {
            throw new("Indentation can not be less than 0");
        }

        return this;
    }

    public CodeBuilder Add(string line)
    {
        this.result.Append(this.GetIndentation).AppendLine(line);
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
                    this.indentation += 1;
                    continue;
                case "<-":
                    this.indentation -= 1;
                    continue;
                case "--":
                    continue;
                default:
                    Add(s);
                    break;
            }
        }

        return this;
    }


    public CodeBuilder AddLineBreak()
    {
        this.result.AppendLine();
        return this;
    }

    public CodeBuilder AddIf(bool condition, Func<string> add) => condition ? this.Add(add()) : this;

    public CodeBuilder AddIf(bool condition, Action<CodeBuilder> builder)
    {
        if (condition)
        {
            builder(this);
        }

        return this;
    }

    public override string ToString() => string.Join("\r\n", this.result);

    public CodeBuilder Region(string region, Action<CodeBuilder> action)
    {
        this.Add("#region " + region);

        action(this);

        this.Add("#endregion");

        return this;
    }

    public CodeBuilder Scope(string prefix, Action<CodeBuilder> body)
    {
        this.Add(prefix + "{").Indent();

        body(this);

        this.Unindent().Add("}");
        return this;
    }
}
