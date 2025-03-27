// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global

namespace SweetMock.BuilderTests;

using SweetMock;
using Util;

public class ConstructorTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void NoneNullableValueTypesShouldBePermitted()
    {
        var source = Build.TestClass<MultiCtorClass>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        var code = generate.syntaxTrees.ToArray();

        testOutputHelper.DumpResult(code, generate.diagnostics);

        Assert.Empty(generate.GetWarnings());
    }

    [Fact]
    public void StaticConstructorsDosNotCount()
    {
        var source = Build.TestClass<ISupportedStaticInterfaceMembers>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetWarnings());
    }

    [Fact]
    public void AbstractClassTest()
    {
        var source = Build.TestClass<AbstractClass>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetWarnings());
    }

    /// <summary>
    ///     teste
    /// </summary>
    public class MultiCtorClass
    {
        /// <summary>
        ///     Empty ctor
        /// </summary>
        public MultiCtorClass()
        {
        }


        /// <summary>
        ///     one parameter
        /// </summary>
        /// <param name="name">Name to set</param>
        public MultiCtorClass(string name) => this.Name = name;

        /// <summary>
        ///     Two Parameters
        /// </summary>
        /// <param name="name">Name to set</param>
        /// <param name="age">Age to set</param>
        public MultiCtorClass(string name, int age)
        {
            this.Name = name;
            this.Age = age;
        }

        public string? Name { get; }
        public int Age { get; }
    }

    public interface ISupportedStaticInterfaceMembers
    {
        static ISupportedStaticInterfaceMembers() => StaticProperty = "Set from ctor";

        static string StaticProperty { get; set; }
    }

    public abstract class AbstractClass
    {
    }
}
