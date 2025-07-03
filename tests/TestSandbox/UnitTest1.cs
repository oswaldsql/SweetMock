namespace TestSandbox;

using SweetMock;

public class UnitTest1
{
    [Fact]
    [Mock<ITestInterface>]
    [Mock<MyClass>]
    public void Test1()
    {
        var options = new MockOptions() {Logger = new()};
        
        var sut = Mock.ITestInterface(
            config => config.Test(),
            options: options
            );

        Mock.MyClass(config => config.Name("test"));
        
        sut.Test();
        
        Assert.Single(options.Logger.Test());
    }

    public interface ITestInterface
    {
        void Test();
        
    }
    
    internal class MyClass
    {
        internal virtual string Name() => "Test";
    }
}
