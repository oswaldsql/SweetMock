namespace SweetMock.Generation;

internal static class CodeBuilderExtensions
{
    public static CodeBuilder Apply(this CodeBuilder builder, Action<CodeBuilder> action)
    {
        action(builder);
        return builder;
    }

    public static void AddToConfig(this CodeBuilder source, Action<CodeBuilder> action) =>
        source.Scope("internal partial class Config", action);

    public static CodeBuilder AddConfigMethod(this CodeBuilder source, string name, string[] parameters, Action<CodeBuilder> action) =>
        source.Scope($"public Config {name}({string.Join(", ", parameters)})", builder => builder
            .Apply(action)
            .Add("return this;"));

    public static CodeBuilder Region(this CodeBuilder source, string region, Action<CodeBuilder> action) =>
        source
            .AddUnindented("#region " + region)
            .Apply(action)
            .AddUnindented("#endregion")
            .AddLineBreak();

    public static CodeBuilder Scope(this CodeBuilder source, string prefix, Action<CodeBuilder> action) =>
        source
            .Add(prefix + "{")
            .Indent()
            .Apply(action)
            .Unindent()
            .Add("}");
}
