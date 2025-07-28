namespace SweetMock.Builders;

public class MockBuilder
{
    public IEnumerable<BuildResult> BuildFiles(INamedTypeSymbol target)
    {
        var mockDetails = GetMockDetails(target);

        return BuildResults(mockDetails).ToArray();
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
                    context.AddUnintendedTargetDiagnostic(attributes, "Mocking target contains no members.");
                }

                break;
            }
        }
    }

    public static bool ValidateType(ISymbol? symbol, SourceProductionContext context, IEnumerable<AttributeData> attributes)
    {
        switch (symbol)
        {
            case null:
                context.AddUnsupportedTargetDiagnostic(attributes, "Mocking target must be a class or interface.");
                return false;
            case INamedTypeSymbol { TypeKind: not (TypeKind.Class or TypeKind.Interface) }:
                context.AddUnsupportedTargetDiagnostic(attributes, "Mocking target must be a class or interface.");
                return false;
            case INamedTypeSymbol { IsRecord: true }:
                context.AddUnsupportedTargetDiagnostic(attributes, "Mocking target must not be a record type.");
                return false;
            case INamedTypeSymbol { DeclaredAccessibility: Accessibility.Private }:
                context.AddUnsupportedTargetDiagnostic(attributes, "Mocking target must not be a private class.");
                return false;
            case INamedTypeSymbol { IsSealed: true }:
                context.AddUnsupportedTargetDiagnostic(attributes, "Mocking target must not be a sealed class.");
                return false;
            case INamedTypeSymbol { IsStatic: true }:
                context.AddUnsupportedTargetDiagnostic(attributes, "Mocking target must not be a static class.");
                return false;
            case INamedTypeSymbol { TypeKind: TypeKind.Class } target when target.Constructors.All(t => t.DeclaredAccessibility == Accessibility.Private):
                context.AddUnsupportedTargetDiagnostic(attributes, "Mocking classes must have at least one accessible constructor.");
                return false;
            case INamedTypeSymbol target:
            {
                if (target.GetMembers().Length == 0)
                {
                    context.AddUnintendedTargetDiagnostic(attributes, "Mocking target contains no members.");
                }

                break;
            }
        }

        return true;
    }

    private static IEnumerable<BuildResult> BuildResults(MockDetails mockDetails)
    {
        var code = BaseClassBuilder.Build(mockDetails);
        yield return new("Base", code.ToString());

        var configFiles = ConfigExtensionsBuilder.Build(mockDetails);
        yield return new("Config", configFiles);

        var logFilters = LogExtensionsBuilder.BuildLogExtensions(mockDetails);
        yield return new("Logging", logFilters);

        var factories = FactoryClassBuilder.Build(mockDetails);
        yield return new("Factory", factories);
    }

    private static MockDetails GetMockDetails(INamedTypeSymbol target)
    {
        var sourceName = target.ToString();
        var interfaceNamespace = target.ContainingNamespace.ToString();
        var mockType = "MockOf_" + target.Name;
        var mockName = "MockOf_" + target.Name;
        var constraints = "";

        var typeArguments = target.TypeArguments;
        if (typeArguments.Length > 0)
        {
            var types = string.Join(", ", typeArguments.Select(t => t.Name));
            mockType = $"MockOf_{target.Name}<{types}>";
            constraints = typeArguments.ToConstraints();
        }

        return new(target, interfaceNamespace, sourceName, mockType, mockName, constraints);
    }

    public record BuildResult(string Name, string Content);
}
