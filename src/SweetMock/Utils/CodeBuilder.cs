namespace SweetMock.Utils;

using System;
using System.Collections.Generic;

internal class CodeBuilder(string prepend = "") : IDisposable
{
    private readonly List<string> lines = [];

    public bool IsEmpty => this.lines.Count == 0;

    public int Indentation { get; private set; }

    public void Dispose()
    {
        if (this.Indentation != 0)
        {
            throw new("Indentation can not be zero");
        }
    }

    public CodeBuilder Indent()
    {
        this.Indentation = this.Indentation + 1;
        return this;
    }

    public CodeBuilder Unindent()
    {
        this.Indentation = this.Indentation - 1;

        if (this.Indentation < 0)
        {
            throw new("Indentation can not be less than 0");
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
                    this.Indentation = this.Indentation + 1;
                    continue;
                case "<-":
                    this.Indentation = this.Indentation - 1;
                    continue;
                case "--":
                    continue;
                default:
                    this.lines.Add(prepend + new string(' ', this.Indentation * 4) + s);
                    break;
            }
        }

        return this;
    }

    public CodeBuilder Add(bool condition, Func<string> add) => condition ? this.Add(add()) : this;

    public CodeBuilder Condition(bool condition, Action<CodeBuilder> add)
    {
        if (condition) add(this);
        return this;
    }


    public CodeBuilder On(Func<bool> predicate) => predicate() ? this : new();
    public CodeBuilder On(bool condition) => condition ? this : new();

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

    public CodeBuilder Scope(string prefix, Action<CodeBuilder> body)
    {
        this.Add(prefix + "{").Indent();

        body(this);

        this.Unindent().Add("}");
        return this;
    }
}
