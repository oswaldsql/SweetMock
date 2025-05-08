// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global

namespace SweetMock.BuilderTests.Diagnostics;

public class DiagnosticsSm0003Tests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void RefPropertyTests()
    {
        var source = Build.TestClass<IRefProperty>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        var diagnostics = generate.GetErrors();

        var actual = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, actual.Severity);
        Assert.Equal("SM0003", actual.Id);
        Assert.Equal("Ref property not supported for 'Name' in 'IRefProperty'", actual.GetMessage());

        Assert.StartsWith("Mock<", actual.Location.GetCode());
        Assert.EndsWith(".IRefProperty>", actual.Location.GetCode());
    }

    [Fact]
    public void MethodWithRefReturnTypeShouldRaiseError()
    {
        var source = Build.TestClass<IRefMethod>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        var diagnostics = generate.GetErrors();

        var actual = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, actual.Severity);
        Assert.Equal("SM0003", actual.Id);
        Assert.Equal("Ref return type not supported for 'GetName' in 'IRefMethod'", actual.GetMessage());

        Assert.StartsWith("Mock<", actual.Location.GetCode());
        Assert.EndsWith(".IRefMethod>", actual.Location.GetCode());
    }

    public interface IRefProperty
    {
        ref string Name { get; }
    }

    public interface IRefMethod
    {
        ref string GetName();
    }
}