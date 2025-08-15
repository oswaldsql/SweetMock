namespace Repo;

using SweetMock;

public class Repo
{
}

public interface IRepo2
{
    public void SomeOverload();
    public void SomeOverload(string name);
    public void SomeOverload(int number);
}

//[Mock<ConfigTest>]
public class ConfigTest
{
    public virtual void Config(){}
}
