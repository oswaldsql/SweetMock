namespace SweetMock.BuilderTests.Diagnostics;

using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Util;

public class DiagnosticsSM0002Tests(ITestOutputHelper testOutputHelper) {

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
        Assert.Equal("Mocking target contains no members.", actual.GetMessage());

        Assert.Equal("Mock<SweetMock.BuilderTests.Diagnostics.DiagnosticsSM0002Tests.IEmptyInterface>", actual.Location.GetCode());
    }

    internal interface IEmptyInterface { }
}