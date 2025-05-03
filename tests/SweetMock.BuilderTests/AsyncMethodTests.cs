// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global

namespace SweetMock.BuilderTests;

using System.ComponentModel;
using SweetMock;
using Util;

public class MemberTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData("public string Test { get; set; } = \"something\";", true)]
    [InlineData("public virtual T SomeProperty { get; set; } = new T();", true)]
    [InlineData("public virtual T this[string key] { get => new(); set { } }", true)]
    [InlineData("public virtual T SomeMethod(T input) { return input; }", true)]
    [InlineData("public virtual T SomeMethod<U>(U input) { return new T(); }", true)]
    [InlineData("public virtual void SomeMethod<U>(U input) { return; }", true)]
    [InlineData("public virtual void OutMethod(out T output) { output = new T(); }", true)]
    [InlineData("public virtual IEnumerable<T> SomeList() => [];", true)]
    //[InlineData("public virtual IEnumerable<U> SomeList<U>() => [];", true)]
    [InlineData("public virtual void ActionMethod(Action<T> action) => action(new T());", true)]
    [InlineData("public virtual Task<T> SomeMethodAsync() => Task.FromResult(new T());", true)]
    [InlineData("public virtual event EventHandler<Guid>? SomeEvent;", true)]
    public void TestMemberCompiles(string member, bool sucess)
    {
        var sourceCode = $@"
#nullable enable
namespace Demo;

using SweetMock.BuilderTests;
using SweetMock;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[Mock<Source<System.Guid>>]
public class TestClass{{
    public void Test() {{
       var actual = Mock.Source<System.Guid>();
    }}
}}

public class Source<T> where T : new(){{
    {member}
}}

";
        
        var generate = new SweetMockSourceGenerator().Generate(sourceCode);
        
        output.DumpResult(generate);

        Assert.Empty(generate.GetErrors());
    }
}

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