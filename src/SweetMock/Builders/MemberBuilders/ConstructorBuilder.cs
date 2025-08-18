﻿namespace SweetMock.Builders.MemberBuilders;

using Generation;
using Utils;

/// <summary>
/// Represents a builder for constructing mock constructors.
/// </summary>
internal class ConstructorBuilder(MockContext context) {
    public static void Render(CodeBuilder classScope, MockContext context, IEnumerable<IMethodSymbol> constructors)
    {
        var builder = new ConstructorBuilder(context);
        builder.Build(classScope, constructors);
    }

    public void Build(CodeBuilder classScope, IEnumerable<IMethodSymbol> constructors)
    {
        var distinctConstructors = constructors.Distinct(SymbolEqualityComparer.Default).OfType<IMethodSymbol>().ToArray();

        classScope.Region("Constructors", builder =>
        {
            builder
                .Add("SweetMock.MockOptions? _sweetMockOptions {get;set;}")
                .Add("string _sweetMockInstanceName {get; set;} = \"\";");
            if (distinctConstructors.Length != 0)
            {
                this.BuildConstructors(builder, distinctConstructors);
            }
            else
            {
                this.BuildEmptyConstructor(builder);
            }
        });
    }

    private void BuildConstructors(CodeBuilder builder, IEnumerable<IMethodSymbol> constructors)
    {
        foreach (var constructor in constructors)
        {
            var parameterList = constructor.Parameters.ToString(p => $"{p.Type} {p.Name}, ", "");
            var baseArguments = constructor.Parameters.ToString(p => p.Name);

            var constructorSignature = $"internal protected MockOf_{context.Source.Name}({parameterList}System.Action<{context.ConfigName}>? config = null, SweetMock.MockOptions? options = null) : base({baseArguments})";

            builder.Scope(constructorSignature, ctor => ctor
                .Add("_sweetMockOptions = options ?? SweetMock.MockOptions.Default;")
                .Add("_sweetMockCallLog = _sweetMockOptions.Logger;")
                .Add($"_sweetMockInstanceName = _sweetMockOptions.InstanceName ?? \"{context.Source.Name}\";")
                .BuildLogSegment(constructor)
                .Add($"new {context.ConfigName}(this, config);")
            );
        }
    }

    private void BuildEmptyConstructor(CodeBuilder builder)
    {
        builder.Scope($"internal protected MockOf_{context.Source.Name}(System.Action<{context.ConfigName}>? config = null, SweetMock.MockOptions? options = null)", methodScope => methodScope
            .Add("_sweetMockOptions = options ?? SweetMock.MockOptions.Default;")
            .Add("_sweetMockCallLog = options?.Logger;")
            .Add($"_sweetMockInstanceName = _sweetMockOptions.InstanceName ?? \"{context.Source.Name}\";")
            .Scope("if(_sweetMockCallLog != null)", b2 => b2
                .Add($"_sweetMockCallLog.Add(\"{context.Source}.{context.Source.Name}()\");"))
            .Add($"new {context.ConfigName}(this, config);"));
    }
}
