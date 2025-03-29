// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global

namespace SweetMock.BuilderTests;

using System.ComponentModel;
using SweetMock;
using Util;

public class AsyncMethodTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void SimpleTaskMethodsTests()
    {
        var source = Build.TestClass<ISimpleTaskMethods>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetErrors());
    }

    [Fact]
    public void GenericTaskMethodsTests()
    {
        var source = Build.TestClass<IGenericTaskMethods>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetErrors());
    }

    [Fact]
    public void CanCreateMockForINotifyPropertyChanged()
    {
        var source = Build.TestClass<INotifyPropertyChanged>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetErrors());
    }

    public interface ISimpleTaskMethods
    {
        Task WithParameter(string name);
        Task WithoutParameter();
    }

    public interface IGenericTaskMethods
    {
        Task<string> WithParameter(string name);
        Task<int> WithoutParameter();
    }
}

public class Sub
{
    [Fact]
    public void METHOD()
    {
        // Arrange
        var namedTypeSymbol = SymbolHelper.GetClassSymbol("public class Name {}", "Name");

        // ACT
        Console.WriteLine(namedTypeSymbol);
        
        // Assert 
    }
}