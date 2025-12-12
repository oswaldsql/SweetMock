namespace SweetMock.Builders.MemberBuilders;

using Generation;
using Utils;

internal partial class EventBuilder
{
    public class EventMetadata(IEventSymbol symbol)
    {
        public string Name { get; } = symbol.Name;

        public string ArgumentString { get; } = string.Join(" , ", ((INamedTypeSymbol)symbol.Type).DelegateInvokeMethod!.Parameters.Skip(1).Select(t => t.Type.ToDisplayString(Format.ToFullNameFormatWithGlobal)));

        public string ReturnTypeString { get; } = symbol.Type.ToDisplayString(Format.ToFullNameFormatWithGlobalWithoutNull);

        public IEventSymbol Symbol { get; } = symbol;

        public string ContainingSymbolString { get; } = symbol.ContainingType.ToDisplayString(Format.ToFullNameFormatWithGlobal);

        public string ToSeeCRef { get; } = symbol.ToSeeCRef();

        public string AccessibilityString { get; } = symbol.AccessibilityString();
    }
}
