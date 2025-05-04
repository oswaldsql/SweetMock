namespace SweetMock.BuilderTests;

public interface IMethodRepository
{
    Task<Guid> AddG(string name);
    Task Add(string name);

    void Drop();
    void DropThis(string name);
    string ReturnValue();
    Guid CreateNewCustomer(string name);

    (string name, int age) GetCustomerInfo(string name);

    void Unlike() { }

    static string StaticMethod() => "StaticMethod";

    public string DefaultImp() => "Test";
}