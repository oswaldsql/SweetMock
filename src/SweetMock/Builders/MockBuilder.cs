namespace SweetMock.Builders;

using Generation;

public class MockBuilder
{
    public string BuildFiles(INamedTypeSymbol target, out MockContext context)
    {
        var mockContext = new MockContext(target);

        var result = new CodeBuilder();

        result.AddFileHeader();

        result
            .Nullable()
            .Usings("global::System.Linq","global::System")
            .Scope($"namespace {mockContext.Source.ContainingNamespace}", namespaceScope =>
            {
                var builder = new BaseClassBuilder(mockContext);
                builder.BuildMockClass(namespaceScope)
                    .AddLineBreak()
                    .BuildLogExtensionsClass(mockContext);
            });

        context = mockContext;
        return result.ToString();
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
