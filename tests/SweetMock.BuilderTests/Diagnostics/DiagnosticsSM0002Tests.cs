namespace SweetMock.BuilderTests.Diagnostics;

public class DiagnosticsSm0002Tests(ITestOutputHelper testOutputHelper) {

    [Fact]
    public void MockingEmptyInterfaceWillRaiseTheSm0002Info()
    {
        var source = Build.TestClass<IEmptyInterface>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        var diagnostics = generate.diagnostics;

        var actual = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Info, actual.Severity);
        Assert.Equal("SM0002", actual.Id);
        Assert.Equal("Mocking target 'SweetMock.BuilderTests.Diagnostics.DiagnosticsSm0002Tests.IEmptyInterface' contains no members.", actual.GetMessage());

        Assert.Equal("Mock<SweetMock.BuilderTests.Diagnostics.DiagnosticsSm0002Tests.IEmptyInterface>", actual.Location.GetCode());
    }

    internal interface IEmptyInterface { }
}