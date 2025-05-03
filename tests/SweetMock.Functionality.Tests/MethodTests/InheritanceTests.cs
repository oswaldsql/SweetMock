 // ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global

namespace Test.MethodTests;

public class InheritanceTests
{
    [Fact(Skip = ("New keyword doesn't work right now"))]
//    [Mock<IDerived>]
    public void FactMethodName()
    {
//        var sut = Mock.IDerived(config => config
//            .ReturnBool((MockOf_IDerived.Config.DelegateFor_ReturnBool)(() => true))
//            .ReturnBool((MockOf_IDerived.Config.DelegateFor_ReturnBool_2)(() => true))
////            .Method5(name => throw new Exception())
////            .Method5A(name => throw new Exception())
//        );
//
//        Assert.True(sut.ReturnBool());
//        Assert.True(((IBase)sut).ReturnBool());
    }

    [Fact(Skip = "New keyword nor working right now")]
    //[Mock<IDerived>]
    public void FactMethodName2()
    {
//        var sut = Mock.IDerived(config => config.Method6((MockOf_IDerived.Config.DelegateFor_Method6)(() => "Mocked")));
//
//        Assert.Equal((string?)"Mocked", (string?)sut.Method6());
//        Assert.Equal((string?)"Mocked", (string?)sut.Method6());
//        Assert.Equal("Mocked", ((IBase)sut).Method6());
    }

    public interface IBase
    {
        bool ReturnBool();
        void Method2();

        void Method8(string name);

        Task Method3(string name);

        Task Method4();

        Task<string> Method5(string name);
        Task<string> Method7();

        ValueTask Method3A(string name);
        ValueTask Method4A();
        ValueTask<string> Method5A(string name);
        ValueTask<string> Method7A();


        string Method6() => "base";

        bool Method7(ref string name);
    }

    public interface IDerived : IBase
    {
        new bool ReturnBool();
        new void Method2();
        new void Method8(string name);

        new Task Method3(string name);

        new Task Method4();

        new Task<string> Method5(string name);

        new Task<string> Method7();

        new ValueTask Method3A(string name);
        new ValueTask Method4A();
        new ValueTask<string> Method5A(string name);
        new ValueTask<string> Method7A();

        new string Method6() => "Derived ";

        new bool Method7(ref string name);
    }
}

internal static class ConfigExtensions
{
    /// <summary>
    ///     Configures the mock to throw the specified exception when the method is called.
    ///     Configures <see cref="InheritanceTests.IDerived.Method5A" />, <see cref="InheritanceTests.IBase.Method5A" />
    /// </summary>
    /// <param name="config">Configuration to add to</param>
    /// <param name="throws">Exception to throw</param>
    /// <returns>The updated configuration.</returns>
  //  public static MockOf_IDerived.Config Method5Aext2(this MockOf_IDerived.Config config, System.Exception throws)
  //  {
  //      config.Method5A(call: (MockOf_IDerived.Config.DelegateFor_Method5A)((string _) => throw throws));
  //      config.Method5A(call: (MockOf_IDerived.Config.DelegateFor_Method5A_2)((string _) => throw throws));
  //      return config;
  //  }
  //
  //  public static MockOf_IDerived.Config ReturnBool2(this MockOf_IDerived.Config config, System.Exception throws) {
  //      config.ReturnBool(call:(MockOf_IDerived.Config.DelegateFor_ReturnBool)(() => throw throws));// line : 150
  //      config.ReturnBool(call:(MockOf_IDerived.Config.DelegateFor_ReturnBool_2)(() => throw throws));// line : 150
  //      return config;
  //  }
}
