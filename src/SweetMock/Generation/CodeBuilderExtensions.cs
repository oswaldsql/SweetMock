namespace SweetMock.Generation;

using System;

internal static class CodeBuilderExtensions
{
    public static void AddToConfig(this CodeBuilder source, Action<CodeBuilder> action)
    {
        source.Scope("internal partial class Config", action);
    }

    public static CodeBuilder AddConfigMethod(this CodeBuilder source, string name, string[] parameters, Action<CodeBuilder> action)
    {
        source.Add($$"""public Config {{name}}({{string.Join(", ", parameters)}}) {""").Indent();
        action(source);
        source.Add("return this;");
        source.Unindent().Add("}");

        return source;
    }

    public static CodeBuilder Region(this CodeBuilder source, string region, Action<CodeBuilder> action)
    {
        source.Add("#region " + region);

        action(source);

        source.Add("#endregion");

        return source;
    }

    public static CodeBuilder Scope(this CodeBuilder source, string prefix, Action<CodeBuilder> body)
    {
        source.Add(prefix + "{").Indent();

        body(source);

        source.Unindent().Add("}");

        return source;
    }
}
