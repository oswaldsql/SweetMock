namespace SweetMock.FixtureTests;

using CustomMocks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SweetMock;

[Fixture<ShoppingBasket>]
[Mock<TimeProvider, MockOf_TimeProvider>]
[Mock<ILogger<string>, MockOf_ILogger<string>>]
[Mock<Tests>]
[Mock<ShoppingBasket>]
[Mock<ISendEndpoint>]
public class Tests
{
    [SetUp]
    public void Setup()
    {
        var fixture = Fixture.ShoppingBasket(config =>
        {
            config.name = "Tester32";
            config.user.GetUserName("fds");
            config.time.Value = TimeProvider.System;
        });
        var sut = fixture.CreateSut();
        
        Console.WriteLine(sut.Name);
        
        foreach (var callLogItem in fixture.Log.GetLogs())
        {
            Console.WriteLine(callLogItem);
        }
    }
}

public interface IG<T>
{
//    void Do<TU>(TU u);
    void Do2<TU>(Func<TU> u);
}

public class ShoppingBasket(string name, TimeProvider time, IUser user, IStockHandler stockHandler, IBasketRepo repo, ILogger<ShoppingBasket> logger)
{
    public string Name => name;
}

public interface IBasketRepo { }

public interface IStockHandler { }

public interface IUser
{
    public string GetUserName();
}

public class User : IUser
{
    public string GetUserName() => "";
}

    public interface ISendEndpoint
    {
        Task Send<T>(T message, CancellationToken cancellationToken = default)
            where T : class;

        Task Send<T>(T message, IPipe<SendContext<T>> pipe, CancellationToken cancellationToken = default)
            where T : class;

        Task Send<T>(T message, IPipe<SendContext> pipe, CancellationToken cancellationToken = default)
            where T : class;

        Task Send(object message, CancellationToken cancellationToken = default);

        Task Send(object message, Type messageType, CancellationToken cancellationToken = default);

        Task Send(object message, IPipe<SendContext> pipe, CancellationToken cancellationToken = default);

        Task Send(object message, Type messageType, IPipe<SendContext> pipe, CancellationToken cancellationToken = default);

        Task Send<T>(object values, CancellationToken cancellationToken = default)
            where T : class;

        Task Send<T>(object values, IPipe<SendContext<T>> pipe, CancellationToken cancellationToken = default)
            where T : class;

        Task Send<T>(object values, IPipe<SendContext> pipe, CancellationToken cancellationToken = default)
            where T : class;
    }

    public class SendContext<T>
    {
        
    }
    public class SendContext
    {
    }

    public interface IPipe<T>
    {
    }