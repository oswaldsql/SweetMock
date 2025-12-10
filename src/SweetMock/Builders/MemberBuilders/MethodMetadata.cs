namespace SweetMock.Builders.MemberBuilders;

using Utils;

public class MethodMetadata
{
    public MethodMetadata(IMethodSymbol symbol, MockContext context)
    {
        this.Symbol = symbol;
        this.Context = context;

        this.Name = symbol.Name;
        this.FullName = symbol.ToDisplayString(Format.SignatureOnlyFormat);
        this.FunctionPointer = $"_{this.Name}";



        this.ReturnType = symbol.ReturnType;
        this.ReturnTypeString = symbol.ReturnsVoid ? "void" : symbol.ReturnType.ToDisplayString(Format.ToFullNameFormatWithGlobal);
        this.ReturnString = symbol.ReturnsVoid ? "" : "return ";
        this.ReturnStringWithoutGeneric = symbol.ReturnType is { TypeKind: TypeKind.TypeParameter, ContainingSymbol: IMethodSymbol } ? "global::System.Object" : this.ReturnTypeString;
        this.ReturnsVoid = symbol.ReturnsVoid;
        this.ReturnsGenericTask = this.ReturnTypeString.StartsWith("global::System.Threading.Tasks.Task<");
        this.ReturnsGenericValueTask = this.ReturnTypeString.StartsWith("global::System.Threading.Tasks.ValueTask<");
        this.ReturnTypeDerivedFromGeneric = symbol.IsReturnTypeDerivedFromGeneric();
        this.ReturnsByRef = symbol.ReturnsByRef;

        var parameters = symbol.Parameters.Select(t => new ParameterInfo(t.Type.ToDisplayString(Format.ToFullNameFormatWithGlobal), t.Name, t.OutAsString(), t.Name)).ToList();
        var NamedParameterString = parameters.ToString(p => $"{p.OutString}{p.Type} {p.Name}");

        if (symbol.IsGenericMethod)
        {
            parameters.AddRange(symbol.TypeArguments.Select(typeArgument => new ParameterInfo("global::System.Type", "typeOf_" + typeArgument.Name, "", "typeof(" + typeArgument.Name + ")")));
        }
        this.NameList = parameters.ToString(p => $"{p.OutString}{p.Function}");
        this.DelegateParameters = symbol.GetParameterInfos().ToString(p => $"{p.OutString}{p.Type} {p.Name}");
        this.BlindParameterString = symbol.GetParameterInfos().ToString(p => $"{p.OutString}{p.Type} _");
        this.HasOutParameters = symbol.HasOutParameters();

        this.DelegateName = $"DelegateFor_{symbol.Name}";
        this.DelegateType = symbol is { IsGenericMethod: true, ReturnsVoid: false } ? "object" : this.ReturnTypeString;


        this.CastString = symbol is { IsGenericMethod: true, ReturnsVoid: false } ? " (" + this.ReturnTypeString + ") " : "";
        var GenericString = symbol.GenericString();

        if (symbol.ContainingType.TypeKind == TypeKind.Interface)
        {
            this.Signature = $"{this.ReturnTypeString} global::{symbol.ContainingSymbol}.{this.Name}{GenericString}({NamedParameterString})";
        }
        else
        {
            this.Signature = $"{symbol.AccessibilityString()} override {this.ReturnTypeString} {this.Name}{GenericString}({NamedParameterString})";
        }

        this.ToSeeCRef = symbol.ToSeeCRef();
    }

    public IMethodSymbol Symbol { get; }
    public MockContext Context { get; }
    public int Index { get; set; } = 1;

    public void Initialize(int index)
    {
        if (index != 1)
        {
            this.Index = index;

            this.FunctionPointer = $"_{this.Name}_{index}";
            this.DelegateName = $"DelegateFor_{this.Symbol.Name}_{index}";
        }
    }

    public string Name { get; }
    public string FullName { get; }

    public string Signature { get; }
    public string ToSeeCRef { get; }


    public bool ReturnTypeDerivedFromGeneric { get; }

    public ITypeSymbol ReturnType { get; }

    public bool ReturnsGenericValueTask { get; }

    public bool ReturnsGenericTask { get; }

    public bool ReturnsVoid { get; }

    public string ReturnTypeString { get; }

    public string ReturnString { get; }
    public bool ReturnsByRef { get; }

    public bool HasOutParameters { get; }

    public string ReturnStringWithoutGeneric { get; }

    public string BlindParameterString { get; }


    public string DelegateParameters { get; }

    public string DelegateType { get;  }

    public string DelegateName { get; private set; }

    public string CastString { get; }

    public string NameList { get; }



    public string FunctionPointer { get; private set; }
}
