namespace Test.ConstructorTests;

[Mock<Repo<GenericClassTests>>]
public class GenericClassTests
{
    [Fact]
    public void CanCreateGenericMock()
    {
        Action<Guid> trigger = null!;
        // Arrange & Act
        var callLog = new CallLog();
        var actual = Mock.Repo<Guid>(config => config
            .SomeMethod(Guid.NewGuid())
            .SomeMethod(Guid => Guid)
            .SomeMethod(new ArgumentException())
            .SomeProperty(Guid.NewGuid())
            .SomeProperty(Guid.NewGuid, _ => { })
            .Indexer(new())
            .Indexer(s => Guid.NewGuid(), (s, guid) => { })
            .SomeEvent(Guid.NewGuid())
            .SomeEvent(out trigger).LogCallsTo(callLog)
            .OutMethod((out Guid output) => output = Guid.NewGuid())
            .OutMethod(throws: new ArgumentException())
            .SomeList(() => [Guid.NewGuid()])
            .SomeList(returns: [Guid.NewGuid()])
            //.ActionMethod()
            .SomeMethodAsync(returns: Task.FromResult(Guid.NewGuid()))
        );

        trigger(Guid.NewGuid());

        Assert.NotNull(actual);

        callLog.SomeMethod(args => args.input is Guid);
    }

    public class Repo<T> where T : new()
    {
        public virtual T SomeProperty { get; set; }

        public virtual T this[string key] { get => new(); set { } }

        public virtual T SomeMethod(T input) { return input; }

        public virtual void OutMethod(out T output)
        {
            output = new T();
        }

        public virtual IEnumerable<T> SomeList() => [];

        //public virtual void ActionMethod(Action<T> action) => action(default);

        public virtual Task<T> SomeMethodAsync() => Task.FromResult(new T());
        
        public virtual event EventHandler<Guid>? SomeEvent;

        protected virtual void OnSomeEvent(Guid e)
        {
            SomeEvent?.Invoke(this, e);
        }
    }
}