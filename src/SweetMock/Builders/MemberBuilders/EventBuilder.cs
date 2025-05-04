namespace SweetMock.Builders.MemberBuilders;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Utils;

/// <summary>
///     Represents a builder for events, implementing the ISymbolBuilder interface.
/// </summary>
internal static class EventBuilder
{
    public static void Build(CodeBuilder classScope, IEnumerable<IEventSymbol> events)
    {
        var lookup = events.ToLookup(t => t.Name);
        foreach (var m in lookup)
        {
            BuildEvents(classScope, m.ToArray());
        }
    }

    /// <summary>
    ///     Builds the events based on the given symbols and adds them to the code builder.
    /// </summary>
    /// <param name="classScope"></param>
    /// <param name="eventSymbols">The event symbols to build.</param>
    /// <returns>True if any events were built; otherwise, false.</returns>
    private static void BuildEvents(CodeBuilder classScope, IEventSymbol[] eventSymbols)
    {
        var name = eventSymbols.First().Name;

        classScope.Region($"Event : {name}", builder =>
        {
            var eventCount = 1;
            foreach (var symbol in eventSymbols)
            {
                BuildEvent(builder, symbol, eventCount);
                eventCount++;
            }
        });
    }

    /// <summary>
    ///     Builds an individual event and adds it to the code builder.
    /// </summary>
    /// <param name="builder">The code builder to add the event to.</param>
    /// <param name="symbol">The event symbol to build.</param>
    /// <param name="eventCount">The count of the event being built.</param>
    /// <returns>True if any events were built; otherwise, false.</returns>
    private static void BuildEvent(CodeBuilder builder, IEventSymbol symbol, int eventCount)
    {
        var eventName = symbol.Name;
        var invokeMethod = symbol.Type.GetMembers().OfType<IMethodSymbol>().First(t => t.Name == "Invoke");
        var types = string.Join(" , ", invokeMethod.Parameters.Skip(1).Select(t => t.Type));
        var typeSymbol = symbol.Type.ToString().Trim('?');

        var eventFunction = eventCount == 1 ? eventName : $"{eventName}_{eventCount}";

        var (containingSymbol, accessibilityString, overrideString) = symbol.Overwrites();

        builder.AddLines($$"""

                      private event {{typeSymbol}}? _{{eventFunction}};
                      {{accessibilityString}}{{overrideString}} event {{typeSymbol}}? {{containingSymbol}}{{eventName}}
                      {
                      """);

        builder.Scope("add", b => b
            .BuildLogSegment(symbol.AddMethod, true)
            .Add($"this._{eventFunction} += value;"));

        builder.Scope("remove", b => b
            .BuildLogSegment(symbol.RemoveMethod, true)
            .Add($"this._{eventFunction} -= value;"));

        builder.Add("}");

        builder.AddToConfig(config =>
            {
                config.Documentation(doc => doc
                    .Summary($"Returns a action delegate to invoke when <see cref=\"{symbol.ToCRef()}\"/> should be triggered."));

                if (types == "System.EventArgs")
                    config.AddConfigMethod(eventName, ["out System.Action trigger"], codeBuilder => codeBuilder
                        .Add($"trigger = () => target._{eventName}?.Invoke(target, System.EventArgs.Empty);"));
                else
                    config.AddConfigMethod(eventName, [$"out System.Action<{types}> trigger"], codeBuilder => codeBuilder
                        .Add($"trigger = args => target._{eventName}?.Invoke(target, args);")
                    );
            }
        );
    }

    public static void BuildConfigExtensions(CodeBuilder codeBuilder, MockDetails mock, IEnumerable<IEventSymbol> events)
    {
        foreach (var eventSymbol in events)
        {
            var types = string.Join(" , ", ((INamedTypeSymbol)eventSymbol.Type).DelegateInvokeMethod!.Parameters.Skip(1).Select(t => t.Type));

            codeBuilder.AddLineBreak();

            if (types != "System.EventArgs")
            {
                codeBuilder.Documentation(doc => doc
                    .Summary($"Triggers the event <see cref=\"{eventSymbol.ToCRef()}\"/>.")
                    .Parameter("eventArgs", "The arguments used in the event.")
                    .Returns("The updated configuration object."));

                codeBuilder.AddConfigExtension(mock, eventSymbol, [types + " eventArgs"], builder =>
                {
                    builder.Add($"this.{eventSymbol.Name}(out var trigger);");
                    builder.Add("trigger.Invoke(eventArgs);");
                });
            }
            else
            {
                codeBuilder.Documentation(doc => doc
                    .Summary($"Triggers the event <see cref=\"{eventSymbol.ToCRef()}\"/>.")
                    .Returns("The updated configuration object."));

                codeBuilder.AddConfigExtension(mock, eventSymbol, [], builder =>
                {
                    builder.Add($"this.{eventSymbol.Name}(out var trigger);");
                    builder.Add("trigger.Invoke();");
                });
            }
        }
    }
}
