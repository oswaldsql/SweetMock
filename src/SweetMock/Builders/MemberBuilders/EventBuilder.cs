namespace SweetMock.Builders.MemberBuilders;

using Generation;
using Utils;

/// <summary>
///     Represents a builder for events, implementing the ISymbolBuilder interface.
/// </summary>
internal class EventBuilder(MockContext context)
{
    public static void Render(CodeBuilder classScope, MockContext context, IEnumerable<IEventSymbol> events)
    {
        var builder = new EventBuilder(context);
        builder.Build(classScope, events);
    }

    private void Build(CodeBuilder classScope, IEnumerable<IEventSymbol> events)
    {
        var lookup = events.ToLookup(t => t.Name);
        foreach (var m in lookup)
        {
            this.BuildEvents(classScope, m.ToArray());
        }
    }

    /// <summary>
    ///     Builds the events based on the given symbols and adds them to the code builder.
    /// </summary>
    /// <param name="classScope"></param>
    /// <param name="eventSymbols">The event symbols to build.</param>
    /// <returns>True if any events were built; otherwise, false.</returns>
    private void BuildEvents(CodeBuilder classScope, IEventSymbol[] eventSymbols)
    {
        var name = eventSymbols.First().Name;

        classScope.Region($"Event : {name}", builder =>
        {
            var eventCount = 1;
            foreach (var symbol in eventSymbols)
            {
                this.BuildEvent(builder, symbol, eventCount);
                eventCount++;
            }
        });
    }

    /// <summary>
    ///     Builds an individual event and adds it to the code builder.
    /// </summary>
    /// <param name="regionScope">The code builder to add the event to.</param>
    /// <param name="symbol">The event symbol to build.</param>
    /// <param name="eventCount">The count of the event being built.</param>
    /// <returns>True if any events were built; otherwise, false.</returns>
    private void BuildEvent(CodeBuilder regionScope, IEventSymbol symbol, int eventCount)
    {
        var eventName = symbol.Name;
        var invokeMethod = symbol.Type.GetMembers().OfType<IMethodSymbol>().First(t => t.Name == "Invoke");
        var types = string.Join(" , ", invokeMethod.Parameters.Skip(1).Select(t => t.Type));
        var typeSymbol = symbol.Type.ToString().Trim('?');

        var eventFunction = eventCount == 1 ? eventName : $"{eventName}_{eventCount}";

        var (containingSymbol, accessibilityString, overrideString) = symbol.Overwrites();

        var signature = $"{accessibilityString}{overrideString} event {typeSymbol}? {containingSymbol}{eventName}";
        regionScope
            .Add($"private event {typeSymbol}? _{eventFunction};")
            .Scope(signature, eventScope => eventScope
                .Scope("add", addScope => addScope
                    .BuildLogSegment(context, symbol.AddMethod, true)
                    .Add($"this._{eventFunction} += value;"))
                .Scope("remove", removeScope => removeScope
                    .BuildLogSegment(context, symbol.RemoveMethod, true)
                    .Add($"this._{eventFunction} -= value;"))
            )
            .AddToConfig(context, config =>
                {
                    config.Documentation($"Returns a action delegate to invoke when {symbol.ToSeeCRef()} should be triggered.");

                    if (types == "System.EventArgs")
                    {
                        config
                            .AddConfigMethod(context, eventName, ["out System.Action trigger"], codeBuilder => codeBuilder
                                .Add($"trigger = () => target._{eventName}?.Invoke(target, System.EventArgs.Empty);"));
                    }
                    else
                    {
                        config
                            .AddConfigMethod(context, eventName, [$"out System.Action<{types}> trigger"], codeBuilder => codeBuilder
                                .Add($"trigger = args => target._{eventName}?.Invoke(target, args);")
                            );
                    }

                    this.GenerateEventTriggerConfig(regionScope, symbol);
                }
            );
    }

    private void GenerateEventTriggerConfig(CodeBuilder codeBuilder, IEventSymbol eventSymbol)
    {
        var types = string.Join(" , ", ((INamedTypeSymbol)eventSymbol.Type).DelegateInvokeMethod!.Parameters.Skip(1).Select(t => t.Type));

        codeBuilder.AddLineBreak();

        if (types != "System.EventArgs")
        {
            codeBuilder
                .Documentation(doc => doc
                    .Summary($"Triggers the event {eventSymbol.ToSeeCRef()} directly.")
                    .Parameter("eventArgs", "The arguments used in the event.")
                    .Returns("The updated configuration object."))
                .AddConfigExtension(context, eventSymbol, [types + " eventArgs"], config => config
                    .Add($"this.{eventSymbol.Name}(out var trigger);")
                    .Add("trigger.Invoke(eventArgs);"));
        }
        else
        {
            codeBuilder
                .Documentation(doc => doc
                    .Summary($"Triggers the event {eventSymbol.ToSeeCRef()} directly.")
                    .Returns("The updated configuration object."))
                .AddConfigExtension(context, eventSymbol, [], config => config
                    .Add($"this.{eventSymbol.Name}(out var trigger);")
                    .Add("trigger.Invoke();"));
        }
    }
}
