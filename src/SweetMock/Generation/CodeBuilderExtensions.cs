namespace SweetMock.Generation;

using Builders;

internal static class CodeBuilderExtensions
{
    public static CodeBuilder Apply(this CodeBuilder builder, Action<CodeBuilder> action)
    {
        action(builder);
        return builder;
    }

    public static void AddToConfig(this CodeBuilder source, MockContext context, Action<CodeBuilder> action) =>
        source.Scope($"internal partial class {context.ConfigName}", action);

    public static CodeBuilder AddConfigMethod(this CodeBuilder source, MockContext context, string name, string[] parameters, Action<CodeBuilder> action) =>
        source.Scope($"public {context.ConfigName} {name}({string.Join(", ", parameters)})", builder => builder
            .Apply(action)
            .Add("return this;"));

    internal static void AddConfigExtension(this CodeBuilder result, MockContext context, ISymbol symbol, string[] arguments, Action<CodeBuilder> build)
    {
        var name = symbol.Name;
        if (name == "this[]")
        {
            name = "Indexer";
        }

        var args = string.Join(" , ", arguments);

        result.Add($"public {context.ConfigName} {name}({args})")
            .Add("{").Indent()
            .Apply(build)
            .Add("return this;")
            .Unindent().Add("}");
    }

    internal static void AddConfigLambda(this CodeBuilder result, MockContext context, ISymbol symbol, string[] arguments, Action<CodeBuilder> build)
    {
        var name = symbol.Name;
        if (name == "this[]")
        {
            name = "Indexer";
        }

        var args = string.Join(" , ", arguments);

        result.Add($"public {context.ConfigName} {name}({args}) =>")
            .Indent()
            .Apply(build)
            .Unindent()
            .AddLineBreak();
    }

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

    public static CodeBuilder Lambda(this CodeBuilder source, string prefix, Action<CodeBuilder> action) =>
        source
            .Add(prefix + " =>")
            .Indent()
            .Apply(action)
            .Unindent();

    public static CodeBuilder Usings(this CodeBuilder source, params string[] namespaces)
    {
        foreach (var ns in namespaces)
        {
            source.Add("using " + ns + ";");
        }

        return source.AddLineBreak();
    }

    public static CodeBuilder Nullable(this CodeBuilder source) =>
        source.Add("#nullable enable").AddLineBreak();

    public static void End(this CodeBuilder source) {}
}
