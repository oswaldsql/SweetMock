namespace SweetMock.Builders.MemberBuilders;

using System.Linq;
using Microsoft.CodeAnalysis;
using SweetMock.Utils;

internal class BackstopBuilder : IBaseClassBuilder
{
    public bool TryBuildBase(MockDetails details, CodeBuilder result, ISymbol[] symbols)
    {
        foreach (var symbol in symbols)
        {
            result.Add($"// {symbol} was not handled");
        }

        return true;
    }
}