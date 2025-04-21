namespace SweetMock.Utils;

using System;

internal class SurroundingCodeBuilder : IDisposable
{
    private readonly string append1;
    private readonly CodeBuilder builder;
    private readonly int startIndentation;

    public SurroundingCodeBuilder(CodeBuilder codeBuilder, string prepend = "", string append = "")
    {
        this.builder = codeBuilder;
        this.startIndentation = this.builder.Indentation;
        codeBuilder.AddLines(prepend).Indent();
        this.append1 = append;
    }

    public void Dispose()
    {
        this.builder.Unindent().AddLines(this.append1);
        if (this.startIndentation != this.builder.Indentation)
        {
            throw new("Indentation must be the same");
        }
    }
}
