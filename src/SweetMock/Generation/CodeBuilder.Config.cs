namespace SweetMock.Generation;

using Builders;

internal partial class CodeBuilder
{
    public void AddToConfig(MockContext context, Action<CodeBuilder> action) =>
        this.Scope($"internal partial class {context.ConfigName}", action);

    public CodeBuilder AddConfigMethod(MockContext context, string name, string[] parameters, Action<CodeBuilder> action) =>
        this.Scope($"public {context.ConfigName} {name}({string.Join(", ", parameters)})", builder1 => builder1
            .Apply(action)
            .Add("return this;"));

    internal CodeBuilder AddConfigExtension(MockContext context, ISymbol symbol, string[] arguments, Action<CodeBuilder> build)
    {
        var name = symbol.Name;
        if (name == "this[]")
        {
            name = "Indexer";
        }

        var args = string.Join(" , ", arguments);

        return this.Add($"public {context.ConfigName} {name}({args})")
            .Add("{").Indent(scope => scope
                .Apply(build)
                .Add("return this;")
            ).Add("}");
    }

    internal void AddConfigLambda(MockContext context, ISymbol symbol, string[] arguments, Action<CodeBuilder> build)
    {
        var name = symbol.Name;
        if (name == "this[]")
        {
            name = "Indexer";
        }

        var args = string.Join(" , ", arguments);

        this.Add($"public {context.ConfigName} {name}({args}) =>")
            .Indent(build)
            .BR();
    }
}
