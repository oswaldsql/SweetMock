// ReSharper disable EnforceIfStatementBraces
// ReSharper disable HeuristicUnreachableCode

namespace SweetMock.Generation;

internal class DocumentationBuilder(CodeBuilder builder)
{
    public CodeBuilder Builder { get; } = builder;
}
