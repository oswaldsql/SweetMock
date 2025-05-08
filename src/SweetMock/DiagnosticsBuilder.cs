namespace SweetMock;

internal static class DiagnosticsBuilder
{
    private static Location? GetAttributeLocation(this AttributeData attributeData)
    {
        return attributeData.ApplicationSyntaxReference?.GetSyntax()?.GetLocation();
    }

    private static readonly DiagnosticDescriptor SM0001 = new("SM0001", "Unsupported target", "{0}", "Usage", DiagnosticSeverity.Error, true, helpLinkUri: "http://SweetMock.org/SM0001");
    private static readonly DiagnosticDescriptor SM0002 = new("SM0002", "Unintended target", "{0}", "Usage", DiagnosticSeverity.Info, true, helpLinkUri: "http://SweetMock.org/SM0002");
    private static readonly DiagnosticDescriptor SM0003 = new("SM0003", "Unsupported feature", "{0}", "Usage", DiagnosticSeverity.Error, true, helpLinkUri: "http://SweetMock.org/SM0002");
    private static readonly DiagnosticDescriptor SM9999 = new("SM9999", "Unexpected error", "{0}", "Usage", DiagnosticSeverity.Error, true, helpLinkUri: "http://SweetMock.org/SM9999");

    public static void AddUnsupportedTargetDiagnostic(this SourceProductionContext context, IEnumerable<AttributeData> attributes, string message)
    {
        foreach (var attribute in attributes)
        {
            var location = attribute.GetAttributeLocation();
            var diagnostic = Diagnostic.Create(SM0001, location, message);
            context.ReportDiagnostic(diagnostic);
        }
    }

    public static void AddUnintendedTargetDiagnostic(this SourceProductionContext context, IEnumerable<AttributeData> attributes, string message)
    {
        foreach (var attribute in attributes)
        {
            var location = attribute.GetAttributeLocation();
            var diagnostic = Diagnostic.Create(SM0002, location, message);
            context.ReportDiagnostic(diagnostic);
        }
    }

    public static void AddUnsupportedMethodDiagnostic(this SourceProductionContext context, IEnumerable<AttributeData> attributes, string message)
    {
        foreach (var attribute in attributes)
        {
            var location = attribute.GetAttributeLocation();
            var diagnostic = Diagnostic.Create(SM0003, location, message);
            context.ReportDiagnostic(diagnostic);
        }
    }

    public static void AddUnknownExceptionOccured(this SourceProductionContext context, IEnumerable<AttributeData> attributes, string message)
    {
        foreach (var attribute in attributes)
        {
            var location = attribute.GetAttributeLocation();
            var diagnostic = Diagnostic.Create(SM9999, location, message);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
