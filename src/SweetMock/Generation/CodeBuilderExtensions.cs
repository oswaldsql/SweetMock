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
}
