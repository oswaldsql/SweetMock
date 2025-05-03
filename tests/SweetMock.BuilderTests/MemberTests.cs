namespace SweetMock.BuilderTests;

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