﻿namespace SweetMock.Builders;

using Utils;

public static class BuildInMockBuilder
{
    private static Dictionary<string, Func<string>> buildInMocks = new()
    {
        {"Microsoft.Extensions.Logging.ILogger<TCategoryName>", ILogger},
        {"System.TimeProvider", TimeProvider},
        {"Microsoft.Extensions.Options.IOptions<TOptions>", IOptions},
        {"System.Net.Http.HttpClient", HttpClient}
    };

    internal static IEnumerable<MockInfo> CreateBuildInMocks(List<MockTypeWithLocation> collectedMocks, SourceProductionContext spc)
    {
        var candidates = collectedMocks.Where(t => t.Type != null).ToLookup(t => t.Type, SymbolEqualityComparer.Default);

        foreach (var candidate in candidates)
        {
            if (candidate.Key is INamedTypeSymbol symbol)
            {
                var displayString = symbol.ToDisplayString();
                if (buildInMocks.TryGetValue(displayString, out var func))
                {
                    var source = func();
                    spc.AddSource(symbol.ToCRef() + ".g.cs", source);

                    yield return new MockInfo(symbol, symbol.ContainingNamespace + ".MockOf_" + symbol.Name, MockKind.BuildIn, "MockConfig");
                    collectedMocks.RemoveAll(t => SymbolEqualityComparer.Default.Equals(t.Type, symbol));
                }
            }
        }
    }

    private static string ILogger() =>
        ResourceReader.ReadEmbeddedResource("SweetMock.Builders.BuildInMocks.MockOf_ILogger.cs");

    private static string TimeProvider() =>
        ResourceReader.ReadEmbeddedResource("SweetMock.Builders.BuildInMocks.MockOf_TimeProvider.cs");

    private static string IOptions() =>
        ResourceReader.ReadEmbeddedResource("SweetMock.Builders.BuildInMocks.MockOf_IOptions.cs");

    private static string HttpClient() =>
        ResourceReader.ReadEmbeddedResource("SweetMock.Builders.BuildInMocks.MockOf_HttpClient.cs");

}
