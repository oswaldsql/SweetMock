namespace SweetMock.Builders;

using Generation;
using MemberBuilders;
using Utils;

internal static class LogExtensionsBuilder
{
    internal static CodeBuilder BuildLogExtensionsClass(this CodeBuilder builder, MockContext context)
    {
        var memberGroups = context
            .GetCandidates()
            .Distinct(SymbolEqualityComparer.IncludeNullability)
            .ToLookup(t => t.Name);

        builder
            .Scope($"internal partial class MockOf_{context.Source.Name}{context.Source.GetTypeGenerics()}", c => c
                .Scope($"internal class {context.Source.Name}_Logs(CallLog log, string? instanceName = null)", classScope =>
                {
                    classScope
                        .Add($"public System.Collections.Generic.IEnumerable<ArgumentBase> All() =>")
                        .Add($"    log.Calls.Where(t => instanceName == null || t.InstanceName == instanceName);");

                    foreach (var g in memberGroups)
                    {
                        var gKey = g.Key == "this[]" ? "Indexer" : g.Key;
                        gKey = gKey == ".ctor" ? context.Source.Name : gKey;

                        var argsClass = $"{context.MockName}{context.Source.GetTypeGenerics()}.{gKey}_Arguments";

                        classScope
                            .BR()
                            .Add($"public System.Collections.Generic.IEnumerable<{argsClass}> {gKey}(System.Func<{argsClass}, bool>? filter = null) =>")
                            .Add($"    this.All().OfType<{argsClass}>().Where(filter ?? (_ => true));");
                    }
                })
            )
            .BR();

        return builder;
    }
}
