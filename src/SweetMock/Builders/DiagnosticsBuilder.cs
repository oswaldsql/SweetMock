namespace SweetMock.Builders;

internal static class DiagnosticsBuilder
{
    extension(AttributeData attributeData)
    {
        private Location? GetAttributeLocation() =>
            attributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation();
    }

    private static readonly DiagnosticDescriptor Sm0001 = new("SM0001", "Unsupported target", "{0}", "Usage", DiagnosticSeverity.Error, true, helpLinkUri: "https://github.com/oswaldsql/SweetMock/blob/master/doc/AnalyzerRules/SM0001");
    private static readonly DiagnosticDescriptor Sm0002 = new("SM0002", "Unintended target", "{0}", "Usage", DiagnosticSeverity.Info, true, helpLinkUri: "https://github.com/oswaldsql/SweetMock/blob/master/doc/AnalyzerRules/SM0002");
    private static readonly DiagnosticDescriptor Sm0003 = new("SM0003", "Unsupported feature", "{0}", "Usage", DiagnosticSeverity.Error, true, helpLinkUri: "https://github.com/oswaldsql/SweetMock/blob/master/doc/AnalyzerRules/SM0002");
    private static readonly DiagnosticDescriptor Sm9999 = new("SM9999", "Unexpected error", "{0}", "Usage", DiagnosticSeverity.Error, true, helpLinkUri: "https://github.com/oswaldsql/SweetMock/blob/master/doc/AnalyzerRules/SM9999");

    extension(SourceProductionContext context)
    {
        public void AddUnsupportedTargetDiagnostic(IEnumerable<AttributeData> attributes, string message)
        {
            foreach (var attribute in attributes)
            {
                var location = attribute.GetAttributeLocation();
                var diagnostic = Diagnostic.Create(Sm0001, location, message);
                context.ReportDiagnostic(diagnostic);
            }
        }

        public void AddUnintendedTargetDiagnostic(IEnumerable<AttributeData> attributes, string message)
        {
            foreach (var attribute in attributes)
            {
                var location = attribute.GetAttributeLocation();
                var diagnostic = Diagnostic.Create(Sm0002, location, message);
                context.ReportDiagnostic(diagnostic);
            }
        }

        public void AddUnsupportedMethodDiagnostic(IEnumerable<AttributeData> attributes, string message)
        {
            foreach (var attribute in attributes)
            {
                var location = attribute.GetAttributeLocation();
                var diagnostic = Diagnostic.Create(Sm0003, location, message);
                context.ReportDiagnostic(diagnostic);
            }
        }

        public void AddUnknownExceptionOccured(IEnumerable<AttributeData> attributes, string message)
        {
            foreach (var attribute in attributes)
            {
                var location = attribute.GetAttributeLocation();
                var diagnostic = Diagnostic.Create(Sm9999, location, message);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
