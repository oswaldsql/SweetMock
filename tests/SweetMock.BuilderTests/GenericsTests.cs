// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global

namespace SweetMock.BuilderTests;

using SweetMock;
using Util;

public class GenericsTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void EventInheritanceTests()
    {
        var source = $@"
namespace Demo;

using SweetMock.BuilderTests;
using SweetMock;
using System;


[Mock<SweetMock.BuilderTests.GenericsTests.IGeneric<int, int>>]
public class TestClass{{
    public void Test() {{
       {""}
    }}
}}";

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetWarnings());
    }

    [Theory()]
    [InlineData("where T : struct", "int, int")]
    [InlineData("where T : class", "Baseclass, int")]
    [InlineData("where T : class?", "Baseclass, int")]
    [InlineData("where T : notnull", "int, int")]
    [InlineData("where T : unmanaged", "int, int")]
    [InlineData("where T : new()", "int, int")]
    [InlineData("where T : Baseclass", "Baseclass, int")]
    [InlineData("where T : Baseclass?", "Baseclass, int")]
    [InlineData("where T : IBaseInterface", "IBaseInterface, int")]
    [InlineData("where T : IBaseInterface?", "IBaseInterface, int")]
    [InlineData("where T : U", "int, int")]
//    [InlineData("where T : default")]
//    [InlineData("where T : allows ref struct")]
    public void GenericInterfaceTests(string constraint, string mockAttribute)
    {
        var source = $@"
#nullable enable
namespace Demo;

using SweetMock.BuilderTests;
using SweetMock;
using System;

public class Baseclass {{}}
public interface IBaseInterface {{}}

public interface ISutInterface<T, U> {constraint}
{{

}}

[Mock<ISutInterface<{mockAttribute}>>]
public class TestClass{{
    public void Test() {{
       {""}
    }}
}}";
        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetWarnings());
    }

    [Fact]
    public void GenericMethodsInNoGenericInterfaceIsNotSupported()
    {
        var source = Build.TestClass<IGenericMethod>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetErrors());

//        Assert.Equal("Generic methods in non generic interfaces or classes is not currently supported for 'Parse' in 'IGenericMethod'", error.GetMessage());
//        Assert.Equal("MM0004", error.Id);
    }

    public interface IGenericMethod
    {
        void ReturnGeneric(string value);
        T ReturnGeneric<T>(string value) where T : struct;
        //IEnumerable<T> ReturnDerived<T>(string value) where T : struct;
        void ReturnVoid<T>(string value) where T : struct;
        T ReturnTwoGenerics<T, TU>(string value) where T : struct where TU : struct;
    }

    public interface IGeneric<out TKey, in TValue> where TKey : IComparable<TKey>? //, IEnumerable<string>?
    {
        TKey Method1(TValue value);
//        void Method2(T value);
//        T Method3();
//        T Method4(out T value);
//        T Method5(ref T value);
    }
}
