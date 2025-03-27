namespace SweetMock.Utils;

using System;
using System.Collections.Generic;

internal class CodeBuilder(string prepend = "")
{
    private readonly List<string> lines = [];
    private int indentation;

    public bool IsEmpty => this.lines.Count == 0;

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
            throw new Exception("Indentation can not be less than 0");
        }

        return this;
    }

    public CodeBuilder Add()
    {
        this.lines.Add(prepend);
        return this;
    }

    public CodeBuilder Add(string text)
    {
        var strings = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
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
                default:
                    this.lines.Add(prepend + new string(' ', this.indentation * 4) + s);
                    break;
            }
        }

        return this;
    }

    public CodeBuilder Add(bool condition, Func<string> add) => condition ? this.Add(add()) : this;

    public CodeBuilder On(Func<bool> predicate) => predicate() ? this : new CodeBuilder();
    public CodeBuilder On(bool condition) => condition ? this : new CodeBuilder();

    public CodeBuilder Add(CodeBuilder builder)
    {
        foreach (var text in builder.lines)
        {
            this.Add(text);
        }

        return this;
    }

    public override string ToString() => string.Join("\r\n", this.lines);

    public CodeBuilder Add(List<string> builder)
    {
        foreach (var text in builder)
        {
            this.Add(text);
        }

        return this;
    }

    public SurroundingCodeBuilder Region(string region) => new(this, "#region " + region, "#endregion");
}

internal class SurroundingCodeBuilder : IDisposable
{
    private readonly CodeBuilder builder;
    private readonly string append1;

    public SurroundingCodeBuilder(CodeBuilder codeBuilder, string prepend = "", string append = "")
    {
        builder = codeBuilder;
        codeBuilder.Add(prepend).Indent();
        append1 = append;
    }


    public void Dispose()
    {
        builder.Unindent().Add(append1);
    }
}