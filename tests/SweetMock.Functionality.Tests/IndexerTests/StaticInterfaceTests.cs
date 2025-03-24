// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global

namespace Test.IndexerTests;

public class StaticInterfaceTests
{
    [Fact]
    [Mock<ISupportedStaticInterfaceMembers>]
    public void StaticInterfaceCanBeMocked()
    {
        // Arrange
        var sut = Mock.ISupportedStaticInterfaceMembers();

        // ACT

        // Assert
        Assert.NotNull(sut);
    }

    public interface ISupportedStaticInterfaceMembers
    {
        static ISupportedStaticInterfaceMembers() => StaticProperty = "Set from ctor";

        static string StaticProperty { get; set; }

        static virtual string Bar => "value"; // with implementation
        static string StaticMethod() => "value";
        static event EventHandler? StaticEvent;

        static void DoStaticEvent() => StaticEvent?.Invoke(null, EventArgs.Empty);
    }

    public interface IUnSipportedStaticAbstractInterfaceMembers
    {
        static abstract string AbstractProperty { get; set; }
        static abstract string AbstractMethod();
        static abstract event EventHandler StaticEvent;
    }
}
