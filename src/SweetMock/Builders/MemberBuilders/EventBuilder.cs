namespace SweetMock.Builders.MemberBuilders;

using Generation;

/// <summary>
///     Represents a builder for events, implementing the ISymbolBuilder interface.
/// </summary>
internal partial class EventBuilder(MockInfo mock)
{
    public static void Render(CodeBuilder classScope, MockInfo mock)
    {
        var events = mock.Candidates.OfType<IEventSymbol>();
        var builder = new EventBuilder(mock);
        builder.Build(classScope, events);
    }

    private void Build(CodeBuilder classScope, IEnumerable<IEventSymbol> rawEvents)
    {
        var events = rawEvents.ToLookup(t => t.Name, symbol => new EventMetadata(symbol));
        foreach (var evnt in events)
        {
            this.BuildEvents(classScope, evnt);
        }
    }

    /// <summary>
    ///     Builds the events based on the given symbols and adds them to the code builder.
    /// </summary>
    /// <param name="classScope"></param>
    /// <param name="events">The event symbols to build.</param>
    /// <returns>True if any events were built; otherwise, false.</returns>
    private void BuildEvents(CodeBuilder classScope, IGrouping<string, EventMetadata> events) =>
        classScope.Region($"Event : {events.Key}", builder =>
        {
            CreateLogArgumentsRecord(classScope, events.Key);

            foreach (var evnt in events)
            {
                this.BuildEvent(builder, evnt);
                this.BuildConfigExtensions(builder, evnt);
            }
        });

    private static void CreateLogArgumentsRecord(CodeBuilder classScope, string name) =>
        classScope
            .Add($"public record {name}_Arguments(")
            .Indent(scope => scope
                .Add("global::System.String? InstanceName,")
                .Add("global::System.String MethodSignature")
            )
            .Add($") : ArgumentBase(_containerName, \"{name}\", MethodSignature, InstanceName);")
            .BR();

    private void BuildEvent(CodeBuilder regionScope, EventMetadata evnt)
    {
        var signature = evnt.Symbol.ContainingType.TypeKind == TypeKind.Interface ?
            $"event {evnt.ReturnTypeString}? {evnt.ContainingSymbolString}.{evnt.Name}"
            :$"{evnt.AccessibilityString} override event {evnt.ReturnTypeString}? {evnt.Name}";

        regionScope
            .Add($"private event {evnt.ReturnTypeString}? _{evnt.Name};")
            .Scope(signature, eventScope => eventScope
                .Scope("add", addScope => addScope
                    .Add($"this._log(new {evnt.Name}_Arguments(this._sweetMockInstanceName, \"add\"));")
                    .Add($"this._{evnt.Name} += value;"))
                .Scope("remove", removeScope => removeScope
                    .Add($"this._log(new {evnt.Name}_Arguments(this._sweetMockInstanceName, \"remove\"));")
                    .Add($"this._{evnt.Name} -= value;"))
            )
            .BR();
    }

    private void BuildConfigExtensions(CodeBuilder regionScope, EventMetadata evnt) =>
        regionScope
            .AddToConfig(mock, configScope =>
                {
                    this.GenerateTriggerMethod(configScope, evnt);
                    this.GenerateEventTriggerConfig(configScope, evnt);
                }
            );

    private void GenerateTriggerMethod(CodeBuilder config, EventMetadata evnt)
    {
        if (evnt.ArgumentString == "global::System.EventArgs")
        {
            config
                .Documentation($"Returns a action delegate to invoke when {evnt.ToSeeCRef} should be triggered.")
                .AddConfigMethod(mock, evnt.Name, ["out System.Action trigger"], codeBuilder => codeBuilder
                    .Add($"trigger = () => target._{evnt.Name}?.Invoke(target, System.EventArgs.Empty);"));
        }
        else
        {
            config
                .Documentation($"Returns a action delegate to invoke when {evnt.ToSeeCRef} should be triggered.")
                .AddConfigMethod(mock, evnt.Name, [$"out System.Action<{evnt.ArgumentString}> trigger"], codeBuilder => codeBuilder
                    .Add($"trigger = args => target._{evnt.Name}?.Invoke(target, args);")
                );
        }
    }

    private void GenerateEventTriggerConfig(CodeBuilder codeBuilder, EventMetadata evnt)
    {
        codeBuilder.BR();

        if (evnt.ArgumentString != "global::System.EventArgs")
        {
            codeBuilder
                .Documentation(doc => doc
                    .Summary($"Triggers the event {evnt.ToSeeCRef} directly.")
                    .Parameter("eventArgs", "The arguments used in the event.")
                    .Returns("The updated configuration object."))
                .AddConfigMethod(mock, evnt.Name, [evnt.ArgumentString + " eventArgs"], config => config
                    .Add($"this.{evnt.Name}(out var trigger);")
                    .Add("trigger.Invoke(eventArgs);"));
        }
        else
        {
            codeBuilder
                .Documentation(doc => doc
                    .Summary($"Triggers the event {evnt.ToSeeCRef} directly.")
                    .Returns("The updated configuration object."))
                .AddConfigMethod(mock, evnt.Name, [], config => config
                    .Add($"this.{evnt.Name}(out var trigger);")
                    .Add("trigger.Invoke();"));
        }
    }
}
