namespace TestSandbox;

using SweetMock;

public class UnitTest1
{
    [Fact]
    [Mock<Source>]
    public void Test1()
    {
        var sut = Mock.Source(config => config.SomeValue(returnValues: ["1","2"]));

        sut.SomeValue();
        sut.SomeValue();
//        sut.SomeValue();

    }
}

public interface Source
{
//    public T EmptyGeneric<T>(T name) where T : new();
//    
//    public T ReturnGeneric<T>() where T : new();
//    
//    public void ParameterGeneric<T>(T name); //where T : new();
//
//    public T FullGeneric<T>(T name) where T : new();
//
    //public IEnumerable<T> GenericEnumerable<T>(T name) where T : new();
    public string SomeValue();
}
