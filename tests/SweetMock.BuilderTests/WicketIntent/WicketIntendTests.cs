namespace SweetMock.BuilderTests.WicketIntent;

public class WicketIntendTests(ITestOutputHelper testOutputHelper)
{
    [Fact(Skip="Not yet handled")]
    public void TestIdenticalMethodNamesWithGenericTasks()
    {
        // Arrange
        var source = Build.TestClass<IMembersWhereReturnAndTaskReturnOverlap>();

        // ACT
        var generate = new SweetMockSourceGenerator().Generate(source);

        // Assert
        testOutputHelper.DumpResult(generate);
        Assert.Empty(generate.GetErrors());
    }
    
    [Fact(Skip="Not yet handled")]
    public void TestIMembersWhereReturnAndIEnumerableOverlap()
    {
        // Arrange
        var source = Build.TestClass<IMembersWhereReturnAndIEnumerableOverlap>();

        // ACT
        var generate = new SweetMockSourceGenerator().Generate(source);

        // Assert
        testOutputHelper.DumpResult(generate);
        Assert.Empty(generate.GetErrors());
    }
    
    [Fact(Skip="Not yet handled")]
    public void TestIMembersThatReturnAException()
    {
        // Arrange
        var source = Build.TestClass<IMembersThatReturnAException>();

        // ACT
        var generate = new SweetMockSourceGenerator().Generate(source);

        // Assert
        testOutputHelper.DumpResult(generate);
        Assert.Empty(generate.GetErrors());
    }
    
    [Fact]
    public void TestIMembersThatReturnAOverlappingLambda()
    {
        // Arrange
        var source = Build.TestClass<IMembersThatReturnAOverlappingLambda>();

        // ACT
        var generate = new SweetMockSourceGenerator().Generate(source);

        // Assert
        testOutputHelper.DumpResult(generate);
        Assert.Empty(generate.GetErrors());
    }

        
    [Fact]
    public void TestHttpClient()
    {
        // Arrange
        var source = Build.TestClass<HttpClient>();

        // ACT
        var generate = new SweetMockSourceGenerator().Generate(source);

        // Assert
        testOutputHelper.DumpResult(generate);
        Assert.Empty(generate.GetErrors());
    }
    
    [Fact]
    public void NamespaceAndClassMustBeAbleToHaveSameName()
    {
        // Arrange
        var source = """

                     namespace OverlappingNamespaceAndClass;

                     using SweetMock;
                     using System;

                     public class OverlappingNamespaceAndClass { }

                     public interface IInterface { }

                     [Mock<IInterface>]
                     public class OverlappingNamespaceAndClassTests { }

                     """;

        // ACT
        var generate = new SweetMockSourceGenerator().Generate(source);

        // Assert
        testOutputHelper.DumpResult(generate);
        Assert.Empty(generate.GetErrors());
    }
    
    [Fact]
    public void NamespaceAndClassMustBeAbleToHaveSameName2()
    {
        // Arrange
        var source = """

                     namespace OverlappingNamespaceAndClass;

                     using SweetMock;
                     using System;

                     public class OverlappingNamespaceAndClass { }

                     [Mock<OverlappingNamespaceAndClass>]
                     public class OverlappingNamespaceAndClassTests { }

                     """;

        // ACT
        var generate = new SweetMockSourceGenerator().Generate(source);

        // Assert
        testOutputHelper.DumpResult(generate);
        Assert.Empty(generate.GetErrors());
    }
    
    internal interface IMembersWhereReturnAndTaskReturnOverlap
    {
        public int ReturnInt();
        
        public Task<int> ReturnInt(CancellationToken token);
    }
    
    internal interface IMembersWhereReturnAndIEnumerableOverlap
    {
        public string ReturnStrings();

        public IEnumerable<string> ReturnStrings(int count);
    }
    
    internal interface IMembersThatReturnAException
    {
        public Exception ThrowSomething();
    }
    
    internal interface IMembersThatReturnAOverlappingLambda
    {
        public string this[string key] { get; set; }

        public string Indexer(Func<string, string> get, Action<string, string> set);
    }
}