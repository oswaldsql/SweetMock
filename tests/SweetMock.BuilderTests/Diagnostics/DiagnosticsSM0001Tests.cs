namespace SweetMock.BuilderTests.Diagnostics;

using JetBrains.Annotations;

public class DiagnosticsSm0001Tests(ITestOutputHelper testOutputHelper) {

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

        Assert.Equal("Mock<SweetMock.BuilderTests.Diagnostics.DiagnosticsSm0001Tests.SealedClass>", actual.Location.GetCode());
    }
    
    [Fact]
    public void MockingEnumShouldRaiseTheSm0001Error()
    {
        var source = Build.TestClass<DayOfWeek>();

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
        var source = Build.TestClass("SweetMock.BuilderTests.Diagnostics.DiagnosticsSm0001Tests.StaticClass");

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        var diagnostics = generate.GetErrors();

        var actual = Assert.Single(diagnostics, d => d.Id == "SM0001");
        Assert.Equal(DiagnosticSeverity.Error, actual.Severity);
        Assert.Equal("SM0001", actual.Id);
        Assert.Equal("Mocking target must not be a static class.", actual.GetMessage());

        Assert.Equal("Mock<SweetMock.BuilderTests.Diagnostics.DiagnosticsSm0001Tests.StaticClass>", actual.Location.GetCode());
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

        Assert.Equal("Mock<SweetMock.BuilderTests.Diagnostics.DiagnosticsSm0001Tests.RecordType>", actual.Location.GetCode());
    }
    
    [Fact]
    public void MockingTubelsWillRaiseTheSm0001Error()
    {
        var source = Build.TestClass("(string, int)");

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        var diagnostics = generate.GetErrors();

        var actual = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, actual.Severity);
        Assert.Equal("SM0001", actual.Id);
        Assert.Equal("Mocking target must be a class or interface.", actual.GetMessage());

        Assert.Equal("Mock<(string, int)>", actual.Location.GetCode());
    }
    
    [Fact]
    public void MockingArrayWillRaiseTheSm0001Error()
    {
        var source = Build.TestClass<MyClass[]>();
        
        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        var diagnostics = generate.GetErrors();

        var actual = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, actual.Severity);
        Assert.Equal("SM0001", actual.Id);
        Assert.Equal("Mocking target must be a class or interface.", actual.GetMessage());

        Assert.Equal("Mock<SweetMock.BuilderTests.Diagnostics.DiagnosticsSm0001Tests.MyClass[]>", actual.Location.GetCode());
    }

    [Fact]
    public void ClassWithOnlyPrivateConstructorTest()
    {
        var source = Build.TestClass<ClassWithOnlyPrivateConstructor>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        var diagnostics = generate.GetErrors();

        var actual = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, actual.Severity);
        Assert.Equal("SM0001", actual.Id);
        Assert.Equal("Mocking classes must have at least one accessible constructor.", actual.GetMessage());
    }

    [Fact]
    public void ClassWithOnlyPrivateConstructorTest2()
    {
        var source = Build.TestClass("UnintendedTarget", "", "public class UnintendedTarget{private UnintendedTarget(){}}");

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        var diagnostics = generate.GetErrors();

        var actual = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, actual.Severity);
        Assert.Equal("SM0001", actual.Id);
        Assert.Equal("Mocking classes must have at least one accessible constructor.", actual.GetMessage());
    }

    
    
    /*
     * public class UnintendedTarget
{
    private UnintendedTarget()
    {

    }
}

[Mock<UnintendedTarget>]
public class Test{}
     */
    
    
    /// <summary>
    ///     teste
    /// </summary>
    public class MultiCtorClass
    {
        /// <summary>
        ///     Empty ctor
        /// </summary>
        public MultiCtorClass()
        {
        }


        /// <summary>
        ///     one parameter
        /// </summary>
        /// <param name="name">Name to set</param>
        public MultiCtorClass(string name) => this.Name = name;

        /// <summary>
        ///     Two Parameters
        /// </summary>
        /// <param name="name">Name to set</param>
        /// <param name="age">Age to set</param>
        public MultiCtorClass(string name, int age)
        {
            this.Name = name;
            this.Age = age;
        }

        public string? Name { get; }
        public int Age { get; }
    }

    public interface ISupportedStaticInterfaceMembers
    {
        static ISupportedStaticInterfaceMembers() => StaticProperty = "Set from ctor";

        static string StaticProperty { get; set; }
    }

    public abstract class AbstractClass
    {
    }

    public class ClassWithOnlyPrivateConstructor
    {
        private string Name { get; set; }

        private ClassWithOnlyPrivateConstructor()
        {
            Name = "test";
        }
    }
    
    public class MyClass { }
    
    internal interface IEmptyInterface { }

    private class PrivateClass { }
    
    internal sealed class SealedClass { }
    
    [UsedImplicitly]
    internal static class StaticClass { }
    
    internal record RecordType(string Name);
}