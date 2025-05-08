namespace SweetMock.BuilderTests.Diagnostics;

using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Util;

public class DiagnosticsSM0001Tests(ITestOutputHelper testOutputHelper) {

    [Fact]
    public void MockingSealedClassesWillRaiseTheSm0001Error()
    {
        var source = Build.TestClass<SealedClass>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        var diagnostics = generate.GetErrors();

        var actual = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, actual.Severity);
        Assert.Equal("SM0001", actual.Id);
        Assert.Equal("Mocking target must not be a sealed class.", actual.GetMessage());

        Assert.Equal("Mock<SweetMock.BuilderTests.Diagnostics.DiagnosticsSM0001Tests.SealedClass>", actual.Location.GetCode());
    }
    
    [Fact]
    public void MockingEnumShouldRaiseTheSm0001Error()
    {
        var source = Build.TestClass<System.DayOfWeek>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        var diagnostics = generate.GetErrors();

        var actual = Assert.Single(diagnostics, d => d.Id == "SM0001");
        Assert.Equal(DiagnosticSeverity.Error, actual.Severity);
        Assert.Equal("SM0001", actual.Id);
        Assert.Equal("Mocking target must be a class or interface.", actual.GetMessage());

        Assert.Equal("Mock<System.DayOfWeek>", actual.Location.GetCode());
    }
    
    [Fact]
    public void MockingStaticClassWillRaiseTheSm0001Error()
    {
        var source = Build.TestClass("SweetMock.BuilderTests.Diagnostics.DiagnosticsSM0001Tests.StaticClass");

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        var diagnostics = generate.GetErrors();

        var actual = Assert.Single(diagnostics, d => d.Id == "SM0001");
        Assert.Equal(DiagnosticSeverity.Error, actual.Severity);
        Assert.Equal("SM0001", actual.Id);
        Assert.Equal("Mocking target must not be a static class.", actual.GetMessage());

        Assert.Equal("Mock<SweetMock.BuilderTests.Diagnostics.DiagnosticsSM0001Tests.StaticClass>", actual.Location.GetCode());
    }
        
    [Fact]
    public void MockingPrivateClassWillRaiseTheSm0001Error()
    {
        var source = Build.TestClass("Demo.TestClass.PrivateClass", outsideTest:"private class PrivateClass { }");

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        var diagnostics = generate.GetErrors();

        var actual = Assert.Single(diagnostics, d => d.Id == "SM0001");
        Assert.Equal(DiagnosticSeverity.Error, actual.Severity);
        Assert.Equal("SM0001", actual.Id);
        Assert.Equal("Mocking target must not be a private class.", actual.GetMessage());

        Assert.Equal("Mock<Demo.TestClass.PrivateClass>", actual.Location.GetCode());
    }
    
    [Fact]
    public void MockingRecordTypesWillRaiseTheSm0001Error()
    {
        var source = Build.TestClass<RecordType>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        var diagnostics = generate.GetErrors();

        var actual = Assert.Single(diagnostics, d => d.Id == "SM0001");
        Assert.Equal(DiagnosticSeverity.Error, actual.Severity);
        Assert.Equal("SM0001", actual.Id);
        Assert.Equal("Mocking target must not be a record type.", actual.GetMessage());

        Assert.Equal("Mock<SweetMock.BuilderTests.Diagnostics.DiagnosticsSM0001Tests.RecordType>", actual.Location.GetCode());
    }
    
    internal interface IEmptyInterface { }

    private class privateClass { }
    
    internal sealed class SealedClass { }
    
    [UsedImplicitly]
    internal static class StaticClass { }
    
    internal record RecordType(string name);


}