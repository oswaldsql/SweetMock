namespace TestSandbox;

using SweetMock;

[Mock<IVersionLibrary>]
public class HttpClientMockTests()
{

}

public interface IVersionLibrary
{
    int OverloadedMethod();
    string OverloadedMethod(string name);
    string OverloadedMethod(string name, int value);
    string OverloadedMethod(string name, int? value, DateTime? date);
    string OverloadedMethod(int value, string name);
    string OverloadedMethod<T>(T generic);
}