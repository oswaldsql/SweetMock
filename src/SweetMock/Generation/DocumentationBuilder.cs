// ReSharper disable EnforceIfStatementBraces
// ReSharper disable HeuristicUnreachableCode

namespace SweetMock.Builders;

using Generation;

internal class DocumentationBuilder(CodeBuilder builder)
{
    public CodeBuilder Builder { get; } = builder;
}