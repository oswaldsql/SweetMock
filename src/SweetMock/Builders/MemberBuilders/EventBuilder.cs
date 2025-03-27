namespace SweetMock.Builders.MemberBuilders;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Utils;

/// <summary>
///     Represents a builder for events, implementing the ISymbolBuilder interface.
/// </summary>
internal class EventBuilder : IBaseClassBuilder
{
    public bool TryBuildBase(MockDetails details, CodeBuilder result, ISymbol[] symbols)
    {
        var first = symbols[0];
        if (first is IMethodSymbol { MethodKind: MethodKind.EventAdd or MethodKind.EventRaise or MethodKind.EventRemove }) return true;
        if (first is not IEventSymbol) return false;

        if (!(first.IsAbstract || first.IsVirtual) || first.IsStatic) return true;

        return BuildEvents(result, symbols.OfType<IEventSymbol>());
    }
    
    /// <summary>
    ///     Builds helper methods for the given event symbol.
    /// </summary>
    /// <param name="symbol">The event symbol to build helpers for.</param>
    /// <param name="types">The types of the event parameters.</param>
    /// <param name="eventFunction">The name of the event function.</param>
    /// <returns>An enumerable of helper methods.</returns>
//    private static IEnumerable<ConfigExtension> BuildHelpers(IEventSymbol symbol, string types, string eventFunction)
//    {
//        ConfigExtension cx(string signature, string code, string documentation, [CallerLineNumber] int ln = 0) => new(signature, code + "// line : " + ln, documentation,  symbol.ToString());
//
//        var seeCref = symbol.ToString();
//
//        var eventName = symbol.Name;
//
//        if (types == "System.EventArgs")
//        {
//            yield return cx("out System.Action trigger",
//                $"trigger = () => this.{eventName}();",
//                $"Returns an action that can be used for triggering {eventName}.");
//
//            yield return cx("",
//                $"target.trigger_{eventFunction}(target, System.EventArgs.Empty);",
//                $"Trigger {eventName} directly.");
//        }
//        else
//        {
//            yield return cx($"out System.Action<{types}> trigger",
//                $"trigger = args => this.{eventName}(args);",
//                $"Returns an action that can be used for triggering {eventName}.");
//
//            yield return cx(types + " raise",
//                $"target.trigger_{eventFunction}(target, raise);",
//                $"Trigger {eventName} directly.");
//        }
//    }

    /// <summary>
    ///     Builds the events based on the given symbols and adds them to the code builder.
    /// </summary>
    /// <param name="builder">The code builder to add the events to.</param>
    /// <param name="eventSymbols">The event symbols to build.</param>
    /// <returns>True if any events were built; otherwise, false.</returns>
    private static bool BuildEvents(CodeBuilder builder, IEnumerable<IEventSymbol> eventSymbols)
    {
        var enumerable = eventSymbols as IEventSymbol[] ?? eventSymbols.ToArray();
        var name = enumerable.First().Name;

        using (builder.Region($"Event : {name}"))
        {
            var eventCount = 1;
            foreach (var symbol in enumerable)
                if (BuildEvent(builder, symbol, eventCount))
                    eventCount++;

            return eventCount > 1;
        }
    }

    /// <summary>
    ///     Builds an individual event and adds it to the code builder.
    /// </summary>
    /// <param name="builder">The code builder to add the event to.</param>
    /// <param name="symbol">The event symbol to build.</param>
    /// <param name="helpers">A list of helper methods to add to.</param>
    /// <param name="eventCount">The count of the event being built.</param>
    /// <returns>True if any events were built; otherwise, false.</returns>
    private static bool BuildEvent(CodeBuilder builder, IEventSymbol symbol, int eventCount)
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
                          add
                          {
                      ->
                          {{LogBuilder.BuildLogSegment(symbol.AddMethod, true)}}
                          this._{{eventFunction}} += value;
                      <-
                          }
                          remove {
                      ->
                          {{LogBuilder.BuildLogSegment(symbol.RemoveMethod, true)}}
                          this._{{eventFunction}} -= value;
                      <-
                          }
                      }

                      """);

        builder.Add("internal partial class Config {").Indent();
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
        builder.Unindent().Add("}");

        return true;
    }
}