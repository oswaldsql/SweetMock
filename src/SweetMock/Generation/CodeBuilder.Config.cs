namespace SweetMock.Generation;

using Builders;

internal partial class CodeBuilder
{
    public void AddToConfig(MockContext context, Action<CodeBuilder> action) =>
        this.Scope($"internal partial class {context.ConfigName}", action);

    public CodeBuilder AddConfigMethod(MockContext context, string name, string[] parameters, Action<CodeBuilder> action)
    {
        var arguments = string.Join(", ", parameters);

        return this.Scope($"public {context.ConfigName} {name}({arguments})", builder1 => builder1
            .Apply(action)
            .Add("return this;"));
    }

    internal void AddConfigLambda(MockContext context, string name, string[] arguments, Action<CodeBuilder> build)
    {
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
