// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global

namespace Test.EventTests;

public class EventTests
{
    [Fact]
    [Mock<IEventRepository>]
    public void ItShouldBePossibleToSubscribeAndUnsubscribeToUnmockedEvents()
    {
        // arrange
        var sut = Mock.IEventRepository();

        // act
        sut.CustomEvent += SutOnCustomEvent;
        sut.SimpleEvent += SutOnSimpleEvent;
        sut.EventWithArgs += SutOnEventWithArgs;

        sut.CustomEvent -= SutOnCustomEvent;
        sut.SimpleEvent -= SutOnSimpleEvent;
        sut.EventWithArgs -= SutOnEventWithArgs;

        // assert
        void SutOnCustomEvent(object sender, string eventValue)
        {
            throw new NotImplementedException();
        }

        void SutOnSimpleEvent(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void SutOnEventWithArgs(object? sender, string e)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    [Mock<IEventRepository>]
    public void ItShouldBePossibleToTriggerACustomEvent()
    {
        // arrange
        var actualValue = "";
        IEventRepository? actualSender = null;
        Action<string> eventTrigger = _ => { };
        var sut = Mock.IEventRepository(config => config.CustomEvent(out eventTrigger));

        sut.CustomEvent += SutOnCustomEvent;

        // act
        eventTrigger("event value");

        // assert
        sut.CustomEvent -= SutOnCustomEvent;

        Assert.Equal(sut, actualSender);
        Assert.Equal("event value", actualValue);

        return;

        void SutOnCustomEvent(object sender, string eventValue)
        {
            actualSender = sender as IEventRepository;
            actualValue = eventValue;
        }
    }

    [Fact]
    [Mock<IEventRepository>]
    public void ItShouldBePossibleToTriggerASimpleEvent()
    {
        // arrange
        EventArgs? actualValue = null;
        IEventRepository? actualSender = null;
        var eventTrigger = () => { };
        var sut = Mock.IEventRepository(config => config.SimpleEvent(out eventTrigger));

        sut.SimpleEvent += SutOnSimpleEvent;

        // act
        eventTrigger();

        // assert
        sut.SimpleEvent -= SutOnSimpleEvent;

        Assert.Equal(sut, actualSender);
        Assert.Equal(EventArgs.Empty, actualValue);

        return;

        void SutOnSimpleEvent(object? sender, EventArgs e)
        {
            actualSender = sender as IEventRepository;
            actualValue = e;
        }
    }

    [Fact]
    [Mock<IEventRepository>]
    public void ItShouldBePossibleToTriggerAEventWithArgs()
    {
        // arrange
        var actualValue = "";
        IEventRepository? actualSender = null;

        Action<string> eventTrigger = _ => { };
        var sut = Mock.IEventRepository(config => config.EventWithArgs(out eventTrigger));

        sut.EventWithArgs += SutOnSimpleEvent;

        // act
        eventTrigger("Event value");

        // assert
        sut.EventWithArgs -= SutOnSimpleEvent;

        Assert.Equal(sut, actualSender);
        Assert.Equal("Event value", actualValue);

        return;

        void SutOnSimpleEvent(object? sender, string e)
        {
            actualSender = sender as IEventRepository;
            actualValue = e;
        }
    }

    public interface IEventRepository
    {
        delegate void SampleEventHandler(object sender, string eventValue);

        event EventHandler SimpleEvent;
        event EventHandler<string> EventWithArgs;
        event SampleEventHandler CustomEvent;
    }
}
