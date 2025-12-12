namespace SweetMock.Builders.MemberBuilders;

using Exceptions;
using Generation;

/// <summary>
///     Represents a builder for properties, implementing the ISymbolBuilder interface.
/// </summary>
internal partial class PropertyBuilder(MockInfo mock)
{
    public static void Render(CodeBuilder classScope, MockInfo mock)
    {
        var properties = mock.Candidates.OfType<IPropertySymbol>().Where(t => !t.IsIndexer);

        var builder = new PropertyBuilder(mock);
        builder.Build(classScope,properties);
    }

    private void Build(CodeBuilder classScope, IEnumerable<IPropertySymbol> properties)
    {
        var propertyGroups = properties.ToLookup(t => t.Name, t=> new PropertyMetadata(t));
        foreach (var grouping in propertyGroups)
        {
            this.BuildProperties(classScope, grouping);
        }
    }

    /// <summary>
    ///     Builds properties and adds them to the code builder.
    /// </summary>
    /// <param name="classScope"></param>
    /// <param name="propertyGroup">An array of property symbols representing the properties.</param>
    /// <returns>True if at least one property was built; otherwise, false.</returns>
    private void BuildProperties(CodeBuilder classScope, IGrouping<string, PropertyMetadata> propertyGroup) =>
        classScope.Region($"#region Property : {propertyGroup.Key}", regionScope =>
        {
            CreateLogArgumentsRecord(regionScope, propertyGroup);

            foreach (var property in propertyGroup)
            {
                if (property.Symbol.ReturnsByRef)
                {
                    throw new RefPropertyNotSupportedException(property);
                }

                this.BuildProperty(regionScope, property);
                this.BuildConfigExtensions(regionScope, property);
            }
        });

    private static void CreateLogArgumentsRecord(CodeBuilder classScope, IGrouping<string, PropertyMetadata> propertyGroup) =>
        classScope
            .Add($"public record {propertyGroup.Key}_Arguments(")
            .Indent(scope => scope
                .Add("global::System.String? InstanceName,")
                .Add("global::System.String MethodSignature,")
                .Add($"{propertyGroup.GenerateReturnType()} value = null")
            )
            .Add($") : ArgumentBase(_containerName, \"{propertyGroup.Key}\", MethodSignature, InstanceName);")
            .BR();

    /// <summary>
    ///     Builds a property and adds it to the code builder.
    /// </summary>
    /// <param name="builder">The code builder to add the property to.</param>
    /// <param name="property">The property symbol representing the property.</param>
    private void BuildProperty(CodeBuilder builder, PropertyMetadata property)
    {
        var setType = property.IsInitOnly ? "init" : "set";

        var signature = property.Symbol.ContainingType.TypeKind == TypeKind.Interface ?
            $"{property.TypeString} {property.ContainingSymbolString}.{property.Name}"
            : $"{property.AccessibilityString} override {property.TypeString} {property.Name}";

        builder
            .Scope(signature, propertyScope => propertyScope
                .AddIf(property.HasGet, get => get
                    .Scope("get", getScope => getScope
                        .Add($"this._log(new {property.Name}_Arguments(this._sweetMockInstanceName, \"get\"));")
                        .Add($"return this._{property.Name}_get();")))
                .AddIf(property.HasSet, set => set
                    .Scope(setType, setScope => setScope
                        .Add($"this._log(new {property.Name}_Arguments(this._sweetMockInstanceName, \"set\", value : value));")
                        .Add($"this._{property.Name}_set(value);"))))
            .BR()
            .AddIf(property.HasGet, () => $"private System.Func<{property.TypeString}> _{property.Name}_get {{ get; set; }} = null!;")
            .AddIf(property.HasSet, () => $"private System.Action<{property.TypeString}> _{property.Name}_set {{ get; set; }} = null!;")
            .BR();
    }

    private void BuildConfigExtensions(CodeBuilder builder, PropertyMetadata property) =>
        builder.AddToConfig(mock, config =>
        {
            this.AddGetSetConfiguration(config, property);
            this.AddThrowConfiguration(config, property);
            this.AddValueConfiguration(config, property);
        });

    private void AddGetSetConfiguration(CodeBuilder config, PropertyMetadata property)
    {
        var arguments = property switch
        {
            { IsGetSet: true } => $"global::System.Func<{property.TypeString}> get, global::System.Action<{property.TypeString}> set",
            { IsGetOnly: true } => $"global::System.Func<{property.TypeString}> get",
            { IsSetOnly: true } => $"global::System.Action<{property.TypeString}> set",
            _ => ""
        };

        config.Documentation(doc => doc
                .Summary($"Configures {property.ToSeeCRef} by specifying methods to call when the property is accessed.")
                .ParameterIf(property.HasGet, "get", "Function to call when the property is read.")
                .ParameterIf(property.HasSet, "set", "Function to call when the property is set.")
                .Returns("The updated configuration object."))
            .AddConfigMethod(mock, property.Name, [arguments], codeBuilder => codeBuilder
                .AddIf(property.HasGet, () => $"target._{property.Name}_get = get;")
                .AddIf(property.HasSet, () => $"target._{property.Name}_set = set;")
            );
    }

    private void AddThrowConfiguration(CodeBuilder config, PropertyMetadata property) => config
        .Documentation(doc => doc
            .Summary("Configures the mock to throw the specified exception when the property is accessed.", $"Configures {property.ToSeeCRef}")
            .Parameter("throws", "The exception to be thrown when the property is accessed.")
            .Returns("The updated configuration object."))
        .AddConfigMethod(mock, property.Name, ["System.Exception throws"], codeBuilder => codeBuilder
            .AddIf(property.IsGetSet, () => $"this.{property.Name}(get : () => throw throws, set : _ => throw throws);")
            .AddIf(property.IsGetOnly, () => $"this.{property.Name}(get : () => throw throws);")
            .AddIf(property.IsSetOnly, () => $"this.{property.Name}(set : _ => throw throws);")
        );

    private void AddValueConfiguration(CodeBuilder config, PropertyMetadata property) => config
        .Documentation(doc => doc
            .Summary($"Specifies a value to use for mocking the property {property.ToSeeCRef}.")
            .Parameter("value", "The value to use for the initial value of the property.")
            .Returns("The updated configuration object."))
        .AddConfigMethod(mock, property.Name, [$"{property.TypeString} value"], builder => builder
            .Add($"global::SweetMock.ValueBox<{property.TypeString}> {property.Name}_value = new (value);")
            .AddIf(property.IsGetSet, () => $"this.{property.Name}(get : () => {property.Name}_value.Value, set : ({property.TypeString} value) => {property.Name}_value.Value = value);")
            .AddIf(property.IsGetOnly, () => $"this.{property.Name}(get : () => {property.Name}_value.Value);")
            .AddIf(property.IsSetOnly, () => $"this.{property.Name}(set : ({property.TypeString} value) => {property.Name}_value.Value = value);"));
}
