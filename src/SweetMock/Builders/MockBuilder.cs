namespace SweetMock.Builders;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Utils;

public class MockBuilder
{
    public IEnumerable<BuildResult> BuildFiles(INamedTypeSymbol target)
    {
        try
        {
            ValidateTargetIsValid(target);

            var mockDetails = GetMockDetails(target);

            return BuildResults(mockDetails).ToArray();
        }
        catch (Exception e)
        {
            return [new("Error", "/*" + e + "*/")];
        }
    }

    private static readonly HashSet<TypeKind> validKinds = [TypeKind.Class, TypeKind.Interface, TypeKind.Struct];
    private static void ValidateTargetIsValid(INamedTypeSymbol target)
    {
        if(!validKinds.Contains(target.TypeKind)) throw new InvalidTargetTypeException(target.TypeKind);
        if(target.DeclaredAccessibility == Accessibility.Private) throw new InvalidAccessibilityException(target.DeclaredAccessibility);
        if (target.IsSealed) throw new Exception("Target must be not be sealed");
        if (target.IsStatic) throw new Exception("Target must be not be static");
        if(target.IsAnonymousType) throw new Exception("Target must not be an anonymous type");
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

