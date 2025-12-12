namespace SweetMock.Builders;

internal static class DiagnosticsBuilder
{
    private static Location? GetAttributeLocation(this AttributeData attributeData) =>
        attributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation();

    private static readonly DiagnosticDescriptor Sm0001 = new("SM0001", "Unsupported target", "{0}", "Usage", DiagnosticSeverity.Error, true, helpLinkUri: "https://github.com/oswaldsql/SweetMock/blob/master/doc/AnalyzerRules/SM0001");
    private static readonly DiagnosticDescriptor Sm0002 = new("SM0002", "Unintended target", "{0}", "Usage", DiagnosticSeverity.Info, true, helpLinkUri: "https://github.com/oswaldsql/SweetMock/blob/master/doc/AnalyzerRules/SM0002");
    private static readonly DiagnosticDescriptor Sm0003 = new("SM0003", "Unsupported feature", "{0}", "Usage", DiagnosticSeverity.Error, true, helpLinkUri: "https://github.com/oswaldsql/SweetMock/blob/master/doc/AnalyzerRules/SM0002");
    private static readonly DiagnosticDescriptor Sm9999 = new("SM9999", "Unexpected error", "{0}", "Usage", DiagnosticSeverity.Error, true, helpLinkUri: "https://github.com/oswaldsql/SweetMock/blob/master/doc/AnalyzerRules/SM9999");

    public static void AddUnsupportedTargetDiagnostic(this SourceProductionContext context, IEnumerable<AttributeData> attributes, string message)
    {
        foreach (var location in attributes.Select(t => t.GetAttributeLocation()))
        {
            var diagnostic = Diagnostic.Create(Sm0001, location, message);
            context.ReportDiagnostic(diagnostic);
        }
    }

    public static void AddUnintendedTargetDiagnostic(this SourceProductionContext context, IEnumerable<AttributeData> attributes, string message)
    {
        foreach (var location in attributes.Select(t => t.GetAttributeLocation()))
        {
            var diagnostic = Diagnostic.Create(Sm0002, location, message);
            context.ReportDiagnostic(diagnostic);
        }
    }

    public static void AddUnsupportedMethodDiagnostic(this SourceProductionContext context, IEnumerable<AttributeData> attributes, string message)
    {
        foreach (var location in attributes.Select(t => t.GetAttributeLocation()))
        {
            var diagnostic = Diagnostic.Create(Sm0003, location, message);
            context.ReportDiagnostic(diagnostic);
        }
    }

    public static void AddUnknownExceptionOccured(this SourceProductionContext context, IEnumerable<AttributeData> attributes, string message)
    {
        foreach (var location in attributes.Select(t => t.GetAttributeLocation()))
        {
            var diagnostic = Diagnostic.Create(Sm9999, location, message);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
