namespace Test.ConstructorTests;

[Mock<Repo<GenericClassTests>>]
public class GenericClassTests
{
    [Fact]
    public void CanCreateGenericMock()
    {
        Action<Guid> trigger = null!;
        // Arrange & Act
        CallLog callLog = new CallLog();
        var actual = Mock.Repo<Guid>(config => config
            .SomeMethod(returns: Guid.NewGuid())
            .SomeMethod(call: Guid => Guid)
            .SomeMethod(throws: new ArgumentException())
            .SomeProperty(returns: Guid.NewGuid())
            .SomeProperty(get: Guid.NewGuid, set: Set)
            .Indexer(values: new Dictionary<string, Guid>())
            .Indexer(get: s => Guid.NewGuid(), (s, guid) => { })
            .SomeEvent(Guid.NewGuid())
            .SomeEvent(out trigger).LogCallsTo(callLog)
    );

        trigger(Guid.NewGuid());
    // Assert
        Assert.NotNull(actual);

        callLog.SomeMethod(args => args.input is Guid);
    }

    private void Set(Guid guid)
    {
    }

    public class Repo<T> where T : new()
    {
        public virtual T SomeMethod(T input) => input;
        public virtual T SomeProperty { get; set; }
        public virtual T this[string key] { get { return new T();} set{} }
        public virtual event EventHandler<Guid> SomeEvent;
    }
}