namespace SweetMock.Generation;

internal partial class CodeBuilder
{
    public CodeBuilder Apply(Action<CodeBuilder> action)
    {
        action(this);
        return this;
    }

    public CodeBuilder Region(string region, Action<CodeBuilder> action) =>
        this
            .AddUnindented("#region " + region)
            .Apply(action)
            .AddUnindented("#endregion")
            .BR();

    public CodeBuilder Scope(string prefix, Action<CodeBuilder> action) =>
        this
            .Add(prefix + "{")
            .Indent(indentScope => indentScope
                .Apply(action))
            .Add("}");

    public CodeBuilder Lambda(string prefix, Action<CodeBuilder> action) =>
        this
            .Add(prefix + " =>")
            .Indent(indentScope => indentScope
                .Apply(action));

    public CodeBuilder Usings(params string?[] namespaces) =>
        this
            .AddMultiple(namespaces.Where(t => !string.IsNullOrEmpty(t)), ns => "using " + ns + ";")
            .BR();

    public CodeBuilder Nullable() =>
        this
            .Add("#nullable enable")
            .BR();

    public void End() {}
}
