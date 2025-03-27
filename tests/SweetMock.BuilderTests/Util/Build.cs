namespace SweetMock.BuilderTests.Util;

public static class Build
{
    public static string TestClass<T>(string content = "")
    {
        var name = typeof(T).FullName!.Replace("+", ".");

        Console.WriteLine(name);

        return $@"
namespace Demo;

using SweetMock.BuilderTests;
using SweetMock;
using System;

[Mock<{name}>]
public class TestClass{{
    public void Test() {{
       {content}
    }}
}}";
    }

    public static string TestClass(string interfaceName, string content = "")
    {
        var name = interfaceName;

        return $@"
namespace Demo;

using SweetMock.BuilderTests;
using SweetMock;
using System;

[Mock<{name}>]
public class TestClass{{
    public void Test() {{
       {content}
    }}
}}";
    }
}
