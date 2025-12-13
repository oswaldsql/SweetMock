namespace SweetMock.Exceptions;

internal class UnsupportedAccessibilityException(Accessibility accessibility) : SweetMockException($"Unsupported accessibility type '{accessibility}'")
{
    public Accessibility Accessibility => accessibility;
}
