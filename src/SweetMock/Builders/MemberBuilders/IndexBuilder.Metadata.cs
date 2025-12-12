namespace SweetMock.Builders.MemberBuilders;

using Generation;
using Utils;

internal partial class IndexBuilder
{
    public class IndexMetadata(IPropertySymbol symbol, int index)
    {
        public string ReturnTypeString { get; } = symbol.Type.ToDisplayString(Format.ToFullNameFormatWithGlobal);

        public string TypeName { get; } = symbol.SetMethod?.Parameters[1].Name ?? "";

        public bool HasGet { get; } = symbol.GetMethod != null;

        public bool HasSet { get; } = symbol.SetMethod != null;

        public bool IsGetOnly { get; } = symbol.GetMethod != null && symbol.SetMethod == null;

        public bool IsSetOnly { get; } = symbol.GetMethod == null && symbol.SetMethod != null;

        public bool IsGetSet { get; } = symbol.GetMethod != null && symbol.SetMethod != null;

        public IPropertySymbol Symbol { get; } = symbol;

        public string ContainingSymbolString { get; } = symbol.ContainingType.ToDisplayString(Format.ToFullNameFormatWithGlobal);

        public string ToSeeCRef { get; } = symbol.ToSeeCRef().Replace(".Item[", ".this[");

        public string AccessibilityString { get; } = symbol.AccessibilityString();

        public string KeyTypeString { get; } = symbol.Parameters[0].Type.ToDisplayString(Format.ToFullNameFormatWithGlobal);

        public string KeyName { get; } = symbol.SetMethod?.Parameters[0].Name ?? symbol.GetMethod!.Parameters[0].Name;

        public string InternalName { get; } = index == 1 ? "_onIndex" : $"_onIndex_{index}";

        public bool IsInInterface { get; } = symbol.ContainingType.TypeKind == TypeKind.Interface;

    }
}
