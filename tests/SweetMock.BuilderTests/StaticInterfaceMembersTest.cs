// ReSharper disable ArrangeTypeMemberModifiers

namespace SweetMock.BuilderTests;

using Microsoft.CodeAnalysis;
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

    [Fact]
    public void AbstractStaticMembersAreNotSupported()
    {
        // Arrange
        var source = Build.TestClass("MiniMock.UnitTests.StaticInterfaceMembersTest.IStaticAbstractInterfaceMembers");

        // ACT
        var generate = new SweetMockSourceGenerator().Generate(source);

        // Assert
        //testOutputHelper.DumpResult(generate);
        Assert.Single(generate.diagnostics, t => t.Id == "CS8920");
        var actualAbstractPropertyError = Assert.Single(generate.diagnostics, t => t.Id == "MM0005");
        Assert.Equal(DiagnosticSeverity.Error, actualAbstractPropertyError.Severity);
        Assert.Equal("Static abstract members in interfaces or classes is not supported for 'AbstractMethod' in 'IStaticAbstractInterfaceMembers'", actualAbstractPropertyError.GetMessage());
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
