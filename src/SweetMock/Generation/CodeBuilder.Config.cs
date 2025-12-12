namespace SweetMock.Generation;

using Builders;

internal partial class CodeBuilder
{
    public void AddToConfig(MockInfo mock, Action<CodeBuilder> action) =>
        this.Scope($"internal partial class {mock.ConfigName}", action);

    public CodeBuilder AddConfigMethod(MockInfo mock, string name, string[] parameters, Action<CodeBuilder> action)
    {
        var arguments = string.Join(", ", parameters);

        return this.Scope($"public {mock.ConfigName} {name}({arguments})", builder1 => builder1
            .Apply(action)
            .Add("return this;"));
    }

    internal void AddConfigLambda(MockInfo mock, string name, string[] arguments, Action<CodeBuilder> build)
    {
        if (name == "this[]")
        {
            name = "Indexer";
        }

        var args = string.Join(" , ", arguments);

        this.Add($"public {mock.ConfigName} {name}({args}) =>")
            .Indent(build)
            .BR();
    }
}
