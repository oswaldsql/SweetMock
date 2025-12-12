namespace SweetMock.Builders.MemberBuilders;

using Utils;

internal partial class MethodBuilder
{
    public class MethodMetadata(IMethodSymbol symbol, MockContext context)
    {
        public IMethodSymbol Symbol { get; } = symbol;

        public MockContext Context { get; } = context;

        public string Name { get; } = symbol.Name;

        public string FullName { get; } = symbol.ToDisplayString(Format.SignatureOnlyFormat);

        public string ToSeeCRef { get; } = symbol.ToSeeCRef();


        public bool IsInInterface { get; } = symbol.ContainingType.TypeKind == TypeKind.Interface;


        public string NamedParameterString { get; private set; } = "";

        public string AccessibilityString { get; } = symbol.AccessibilityString();

        public string ContainingSymbol { get; } = symbol.ContainingSymbol.ToDisplayString(Format.ToFullNameFormatWithGlobal);

        public string GenericString { get; } = symbol.GenericString();

        public bool ReturnTypeDerivedFromGeneric { get; } = symbol.IsReturnTypeDerivedFromGeneric();

        public ITypeSymbol ReturnType { get; } = symbol.ReturnType;

        public bool ReturnsGenericValueTask { get; private set; }

        public bool ReturnsGenericTask { get; private set; }

        public bool ReturnsVoid { get; } = symbol.ReturnsVoid;

        public string ReturnTypeString { get; } = symbol.ReturnsVoid ? "void" : symbol.ReturnType.ToDisplayString(Format.ToFullNameFormatWithGlobal);

        public bool ReturnsByRef { get; } = symbol.ReturnsByRef;

        public bool HasOutParameters { get; } = symbol.Parameters.Any(t => t.RefKind == RefKind.Out);

        public string ReturnStringWithoutGeneric { get; private set; } = "";

        public string ParametersString { get; } = symbol.GetParameterInfos().ToString(p => $"{p.OutString}{p.Type} {p.Name}");
        public IParameterSymbol[] Parameters { get; } = symbol.Parameters.ToArray();

        public string DelegateType { get; } = symbol is { IsGenericMethod: true, ReturnsVoid: false } ? "global::System.Object" : symbol.ReturnsVoid ? "void" : symbol.ReturnType.ToDisplayString(Format.ToFullNameFormatWithGlobal);

        public string DelegateName { get; private set; } = $"DelegateFor_{symbol.Name}";

        public string NameList { get; } = symbol.GetParameterInfos().ToString(p => $"{p.OutString}{p.Function}");


        public string FunctionPointer { get; private set; } = $"_{symbol.Name}";

        public void Initialize(int index)
        {
            this.ReturnStringWithoutGeneric = this.Symbol.ReturnType is { TypeKind: TypeKind.TypeParameter, ContainingSymbol: IMethodSymbol } ? "global::System.Object" : this.ReturnTypeString;
            this.ReturnsGenericTask = this.ReturnTypeString.StartsWith("global::System.Threading.Tasks.Task<");
            this.ReturnsGenericValueTask = this.ReturnTypeString.StartsWith("global::System.Threading.Tasks.ValueTask<");

            var parameters = this.Symbol.Parameters.Select(t => new ParameterInfo(t.Type.ToDisplayString(Format.ToFullNameFormatWithGlobal), t.Name, t.OutAsString(), t.Name)).ToList();
            this.NamedParameterString = parameters.ToString(p => $"{p.OutString}{p.Type} {p.Name}");

            if (index != 1)
            {
                this.FunctionPointer = $"_{this.Name}_{index}";
                this.DelegateName = $"DelegateFor_{this.Symbol.Name}_{index}";
            }
        }
    }
}
