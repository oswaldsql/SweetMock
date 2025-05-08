namespace TestSandbox;

using SweetMock;

public class UnitTest1
{
    [Fact]
    [Mock<MyClass[]>]
    public void Test1()
    {
        var sut = Mock.MyClass(config => config.Name("tester"));
        
        sut.Name = "tester";
        
        Assert.Equal("tester", sut.Name);
    }

    
    public partial class MyClass
    {
        public virtual string Name { get; set; }
    }

    public partial class MyClass
    {
        public virtual string Name2 { get; set; }
    }

}

