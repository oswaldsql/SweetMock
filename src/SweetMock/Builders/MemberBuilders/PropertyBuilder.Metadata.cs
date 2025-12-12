namespace SweetMock.Builders.MemberBuilders;

using Utils;

internal partial class PropertyBuilder
{
    public class PropertyMedata(IPropertySymbol symbol)
    {
        public string Name { get; } = symbol.Name;

        public string TypeString { get; } = symbol.Type.ToDisplayString(Format.ToFullNameFormatWithGlobal);

        public bool HasGet { get; } = symbol.GetMethod != null;

        public bool HasSet { get; } = symbol.SetMethod != null;

        public bool IsGetOnly { get; } = symbol.GetMethod != null && symbol.SetMethod == null;

        public bool IsSetOnly { get; } = symbol.GetMethod == null && symbol.SetMethod != null;

        public bool IsGetSet { get; } = symbol.GetMethod != null && symbol.SetMethod != null;

        public bool IsInitOnly { get; } = symbol.SetMethod?.IsInitOnly == true;

        public IPropertySymbol Symbol { get; } = symbol;

        public string ContainingSymbolString { get; } = symbol.ContainingType.ToDisplayString(Format.ToFullNameFormatWithGlobal);

        public string ToSeeCRef { get; } = symbol.ToSeeCRef();

        public string AccessibilityString { get; } = symbol.AccessibilityString();
    }
}
