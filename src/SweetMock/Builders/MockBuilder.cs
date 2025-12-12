namespace SweetMock.Builders;

using Generation;
using MemberBuilders;

public class MockBuilder
{
    public string BuildFiles(INamedTypeSymbol target, out MockInfo mock)
    {
        var mockInfo = MockInfo.Generated(target);
        mock = mockInfo;

        var result = new CodeBuilder();

        return result
            .AddFileHeader()
            .AddResharperDisable()
            .Nullable()
            .Add($"namespace {mockInfo.Namespace};")
            .Usings("global::SweetMock", "System.Linq")
            .BuildBaseClass(mockInfo)
            .BuildLogExtensionsClass(mockInfo)
            .ToString();
    }

    public static bool CanBeMocked(ISymbol? symbol) =>
        symbol switch
        {
            null => false,
            INamedTypeSymbol { TypeKind: not (TypeKind.Class or TypeKind.Interface) } => false,
            INamedTypeSymbol { IsRecord: true } => false,
            INamedTypeSymbol { DeclaredAccessibility: Accessibility.Private } => false,
            INamedTypeSymbol { IsSealed: true } => false,
            INamedTypeSymbol { IsStatic: true } => false,
            INamedTypeSymbol { TypeKind: TypeKind.Class } target when target.Constructors.All(t => t.DeclaredAccessibility == Accessibility.Private) => false,
            _ => true
        };

    public static void DiagnoseType(ISymbol? symbol, SourceProductionContext context, IEnumerable<AttributeData> attributes)
    {
        switch (symbol)
        {
            case null:
                context.AddUnsupportedTargetDiagnostic(attributes, "Mocking target must be a class or interface.");
                break;
            case INamedTypeSymbol { TypeKind: not (TypeKind.Class or TypeKind.Interface) }:
                context.AddUnsupportedTargetDiagnostic(attributes, "Mocking target must be a class or interface.");
                break;
            case INamedTypeSymbol { IsRecord: true }:
                context.AddUnsupportedTargetDiagnostic(attributes, "Mocking target must not be a record type.");
                break;
            case INamedTypeSymbol { DeclaredAccessibility: Accessibility.Private }:
                context.AddUnsupportedTargetDiagnostic(attributes, "Mocking target must not be a private class.");
                break;
            case INamedTypeSymbol { IsSealed: true }:
                context.AddUnsupportedTargetDiagnostic(attributes, "Mocking target must not be a sealed class.");
                break;
            case INamedTypeSymbol { IsStatic: true }:
                context.AddUnsupportedTargetDiagnostic(attributes, "Mocking target must not be a static class.");
                break;
            case INamedTypeSymbol { TypeKind: TypeKind.Class } target when target.Constructors.All(t => t.DeclaredAccessibility == Accessibility.Private):
                context.AddUnsupportedTargetDiagnostic(attributes, "Mocking classes must have at least one accessible constructor.");
                break;
            case INamedTypeSymbol target:
            {
                if (target.GetMembers().Length == 0)
                {
                    context.AddUnintendedTargetDiagnostic(attributes, $"Mocking target '{target}' contains no members.");
                }

                break;
            }
        }
    }
}
