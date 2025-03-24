namespace SweetMock.Builders.MemberBuilders;

using System.Linq;
using Microsoft.CodeAnalysis;
using SweetMock.Utils;

internal class BackstopBuilder : IBaseClassBuilder, ILoggingExtensionBuilder
{
    public bool TryBuildBase(MockDetails details, CodeBuilder result, ISymbol[] symbols)
    {
        foreach (var symbol in symbols)
        {
            result.Add($"// {symbol} was not handled");
        }

        return true;
    }

    public bool TryBuildLoggingExtension(MockDetails details, CodeBuilder result, ISymbol[] symbols)
    {
        result.Add($"// {symbols.First()} was not handled {symbols.Length}");
        return true;
    }
}