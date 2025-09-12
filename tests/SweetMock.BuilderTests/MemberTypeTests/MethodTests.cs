// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global

namespace SweetMock.BuilderTests.MemberTypeTests;

public class MethodTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void MethodRepositoryTests()
    {
        var source = Build.TestClass<IMethodRepository>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetWarnings());
    }

    [Fact]
    public void DefaultImplementationTests()
    {
        var source = Build.TestClass<IDefaultImplementation>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetWarnings());
    }

    [Fact]
    public void GenericTests()
    {
        var source = @"namespace Demo;
using SweetMock.BuilderTests;
using SweetMock;
using System;

[Mock<SweetMock.BuilderTests.MemberTypeTests.MethodTests.IGeneric<string>>(""tst"")]
public class TestClass{
}";

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetWarnings());
    }

    [Fact]
    public void AbstractClassTests()
    {
        var source = Build.TestClass<AbstractClass>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetWarnings());
    }

    [Fact]
    public void InterfaceWithOverloadsTests()
    {
        var source = Build.TestClass<IWithOverloads>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetWarnings());
    }
    
    
    [Fact]
    public void InterfaceWithOverloadsTests2()
    {
        var source = Build.TestClass<IGenericMethods>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetErrors());
    }

    public interface IDefaultImplementation
    {
        public string NotDefault();

        public string DefaultImp() => "Test";
    }

    public interface IGeneric<T>
    {
        public T ReturnGenericType();
        public void GenericParameter(T source);
    }

    public abstract class AbstractClass
    {
        public abstract string AbstractProperty { get; set; }
        public virtual string VirtualProperty { get; set; } = "test";
        public abstract void AbstractMethod();
        public virtual void VirtualMethod() { }
        public virtual Task TaskMethod(string something) { throw new Exception();}
        public virtual ValueTask TaskMethod() { throw new Exception();}
    }

    public interface IWithOverloads
    {
        public void Method();
        public void Method(int i);
        public void Method(string s);
        public void Method(int i, string s);
        public void Method(string s, int i);
        public Task Method(string s, CancellationToken token);
        public Task<int> Method(int i, CancellationToken token);
        public Task MethodAsync();
        public Task MethodAsync(int i);
    }
    
    public interface IGenericMethods
    {
        public T EmptyGeneric<T>(string str, T name) where T : new();
    }
    
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
}
