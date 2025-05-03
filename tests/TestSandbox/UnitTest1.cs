namespace TestSandbox;

using SweetMock;

public class UnitTest1
{
    [Fact]
    [Mock<IDoubleInheritance>]
    public void Test1()
    {
        var sut = Mock.IDoubleInheritance();
    }
}

internal interface IDoubleInheritance : ICollection<string>, IList<string>
{
}
