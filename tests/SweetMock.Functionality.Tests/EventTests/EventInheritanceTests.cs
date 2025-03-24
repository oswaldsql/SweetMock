// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global

namespace Test.EventTests;

public class EventInheritanceTests
{
    [Fact(Skip = "Inheritance is not working")]
    [Mock<IDerivedWithEvent>]
    public void BothDerivedAndBaseEventsAreTriggered()
    {
        // Arrange
        var eventTriggered = false;
        var baseEventTriggered = false;
        Action<string> trigger = _ => { };
        var sut = Mock.IDerivedWithEvent();// (config => config.Event1(trigger));

        sut.Event1 += (_, _) => { eventTriggered = true; };
        ((IBaseWithEvent)sut).Event1 += (_, _) => baseEventTriggered = true;

        // Act
        trigger("EventArgs.Empty");

        // Assert
        Assert.True(eventTriggered);
        Assert.True(baseEventTriggered);
    }

    public interface IBaseWithEvent
    {
        event EventHandler<string> Event1;
    }

    public interface IDerivedWithEvent //: IBaseWithEvent
    {
        new event EventHandler<string> Event1;
    }
}
