namespace TestSandbox;

using SweetMock;

public class UnitTest1
{
    [Fact]
    [Mock<ITestInterface>]
    public void Test1()
    {
        var options = new MockOptions();
        // new MockOptions() { Logger = new CallLog(), LoggerEnabled = true};
        
        var sut = Mock.ITestInterface(
            config => config.Test(),
            options: options
            );

        sut.Test();
        
        Assert.Single(options.Logger.Test());
    }

    public interface ITestInterface
    {
        void Test();
        
    }
}
