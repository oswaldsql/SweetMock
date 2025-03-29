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
    public static CodeBuilder Build(IEnumerable<IEventSymbol> events)
    {
        using CodeBuilder result = new();

        var lookup = events.ToLookup(t => t.Name);
        foreach (var m in lookup)
        {
            result.Add(BuildEvents(m.ToArray()));
        }

        return result;
    }

    /// <summary>
    ///     Builds the events based on the given symbols and adds them to the code builder.
    /// </summary>
    /// <param name="builder">The code builder to add the events to.</param>
    /// <param name="eventSymbols">The event symbols to build.</param>
    /// <returns>True if any events were built; otherwise, false.</returns>
    private static CodeBuilder BuildEvents(IEventSymbol[] eventSymbols)
    {
        using CodeBuilder builder = new();

        var name = eventSymbols.First().Name;

        using (builder.Region($"Event : {name}"))
        {
            var eventCount = 1;
            foreach (var symbol in eventSymbols)
            {
                BuildEvent(builder, symbol, eventCount);
                eventCount++;
            }
        }

        return builder;
    }

    /// <summary>
    ///     Builds an individual event and adds it to the code builder.
    /// </summary>
    /// <param name="builder">The code builder to add the event to.</param>
    /// <param name="symbol">The event symbol to build.</param>
    /// <param name="helpers">A list of helper methods to add to.</param>
    /// <param name="eventCount">The count of the event being built.</param>
    /// <returns>True if any events were built; otherwise, false.</returns>
    private static void BuildEvent(CodeBuilder builder, IEventSymbol symbol, int eventCount)
    {
        var eventName = symbol.Name;
        var invokeMethod = symbol.Type.GetMembers().OfType<IMethodSymbol>().First(t => t.Name == "Invoke");
        var types = string.Join(" , ", invokeMethod.Parameters.Skip(1).Select(t => t.Type));
        var typeSymbol = symbol.Type.ToString().Trim('?');

        var eventFunction = eventCount == 1 ? eventName : $"{eventName}_{eventCount}";

        var (containingSymbol, accessibilityString, _) = symbol.Overwrites();

        builder.Add($$"""

                      private event {{typeSymbol}}? _{{eventFunction}};
                      {{accessibilityString}}event {{typeSymbol}}? {{containingSymbol}}{{eventName}}
                      {
                      """);

        builder.Scope("add", b => b
            .BuildLogSegment(symbol.AddMethod, true)
            .Add($"this._{eventFunction} += value;"));

        builder.Scope("remove", b => b
            .BuildLogSegment(symbol.RemoveMethod, true)
            .Add($"this._{eventFunction} -= value;"));

        builder.Add("}");

        using (builder.AddToConfig())
        {
            if (types == "System.EventArgs")
                builder.Add($$"""
                              public Config {{eventName}}(out System.Action trigger) {
                                  trigger = () => target._{{eventName}}?.Invoke(target, System.EventArgs.Empty);
                                  return this;
                              }
                              """);
            else
                builder.Add($$"""
                              public Config {{eventName}}(out System.Action<{{types}}> trigger) {
                                  trigger = args => target._{{eventName}}?.Invoke(target, args);
                                  return this;
                              }
                              """);
        }
    }
}
