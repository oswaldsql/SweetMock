// ReSharper disable ArrangeTypeMemberModifiers

namespace SweetMock.BuilderTests;

using SweetMock;
using Util;

public class StaticInterfaceMembersTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void SomeStaticMembersAreSupported()
    {
        // Arrange
        var source = Build.TestClass<ISupportedStaticInterfaceMembers>();

        // ACT
        var generate = new SweetMockSourceGenerator().Generate(source);

        // Assert
        testOutputHelper.DumpResult(generate);
        Assert.Empty(generate.GetErrors());
    }

    internal interface ISupportedStaticInterfaceMembers
    {
        static ISupportedStaticInterfaceMembers() => StaticProperty = 10;

        internal static int StaticProperty { get; set; }

        internal static virtual string Bar => "value"; // with implementation
        internal static string StaticMethod() => "value";

        internal static event EventHandler? StaticEvent;
        internal static void DoStaticEvent() => StaticEvent?.Invoke(null, EventArgs.Empty);
    }

    internal interface IStaticAbstractInterfaceMembers
    {
        static abstract string AbstractProperty { get; set; }
        static abstract string AbstractMethod();
        static abstract event EventHandler StaticEvent;
        static string AbstractMethod1() => "value";
    }
}
