namespace SweetMock.Builders;

using Generation;
using MemberBuilders;

internal static class BaseClassBuilderExt
{
    internal static CodeBuilder BuildBaseClass(this CodeBuilder builder, MockInfo mock) =>
        new BaseClassBuilder(mock).BuildMockClass(builder);
}

internal class BaseClassBuilder(MockInfo context)
{
    internal CodeBuilder BuildMockClass(CodeBuilder namespaceScope) =>
        namespaceScope
            .Documentation($"Mock implementation of {context.ToSeeCRef}.", "Should only be used for testing purposes.")
            .AddGeneratedCodeAttrib()
            .Scope($"internal partial class {context.MockType} : {context.NameAndGenerics}{context.Constraints}", classScope =>
            {
                classScope.Add($"private const string _containerName = \"{context.ExtendedTypeFormat}\";").BR();
                this.InitializeConfig(classScope);
                classScope.InitializeLogging(context);
                this.BuildMembers(classScope);
            })
            .BR();

    private void InitializeConfig(CodeBuilder result) =>
        result.Region("Configuration", builder => builder
            .Documentation("Configuration class for the mock.")
            .AddToConfig(context, config => config
                .Add($"private readonly {context.MockType} target;")
                .BR()
                .Documentation(doc => doc
                    .Summary($"Initializes the configuration for {context.ToSeeCRef} instance of the {context.ConfigName} class")
                    .Parameter("target", "The target mock class.")
                    .Parameter("config", "Optional configuration method."))
                .Scope($"public {context.ConfigName}({context.MockType} target, System.Action<{context.ConfigName}>? config = null)", methodScope => methodScope
                    .Add("this.target = target;")
                    .Add("_initialize();")
                    .Add("config?.Invoke(this);")
                )
                .BR()
                .Add("private global::SweetMock.NotExplicitlyMockedException _createException(string name) => new (name, this.target._sweetMockInstanceName);")
                .BR()
                .Scope("private void _initialize()", codeBuilder =>
                {
                    var candidates = context.Candidates.Where(t => t is IMethodSymbol { MethodKind: MethodKind.Ordinary } or IPropertySymbol).ToLookup(t => t.Name);
                    if (candidates.Count > 0)
                    {
                        codeBuilder.Add("this").Indent(lambdaScope =>
                        {
                            foreach (var candidate in candidates)
                            {
                                var key = candidate.Key;
                                switch (candidate.First())
                                {
                                    case IMethodSymbol { MethodKind: MethodKind.Ordinary }:
                                        lambdaScope.Add($".{key}(throws:this._createException(\"{key}\"))");
                                        break;
                                    case IPropertySymbol { IsIndexer: true }:
                                        lambdaScope.Add(".Indexer(throws:this._createException(\"Indexer\"))");
                                        break;
                                    case IPropertySymbol { IsIndexer: false }:
                                        lambdaScope.Add($".{key}(throws:this._createException(\"{key}\"))");
                                        break;
                                }
                            }
                        });

                        codeBuilder.Add(";");
                    }
                })));

    private void BuildMembers(CodeBuilder classScope)
    {
        ConstructorBuilder.Render(classScope, context);

        MethodBuilder.Render(classScope, context);

        PropertyBuilder.Render(classScope, context);

        IndexBuilder.Render(classScope, context);

        EventBuilder.Render(classScope, context);
    }
}
