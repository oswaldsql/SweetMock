// ReSharper disable EmptyConstructor
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local

namespace Test.ConstructorTests;

using System.Reflection;
using Microsoft.Testing.Platform.Logging;

public class AccessLevelTests
{
    [Fact]
    [Mock<AccessLevelTestClass>]
    public void OnlyPublicAndProtectedCtorAreExposed()
    {
        // Arrange

        // ACT
        var actual = typeof(MockOf_AccessLevelTestClass).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        Assert.Equal(2, actual.Length);
    }

    internal class AccessLevelTestClass
    {
        static AccessLevelTestClass() { }

        public AccessLevelTestClass(bool publicCtor) { }

        protected AccessLevelTestClass(string protectedCtor) { }

        internal AccessLevelTestClass(int internalCtor) { }

        private AccessLevelTestClass(double privateCtor) { }
    }
}

public class GenericClassTests
{
    [Fact]
    //[Mock<GenericClass<string>>]
    [Mock<IGenericInterface<string>>]
    public void CanMockGenericClass()
    {
        // Arrange
        var sut = Mock.IGenericInterface<string>(config => config.Passthrough(call: s => s));

        // ACT
        var actual = sut.Passthrough("test2");
        
        // Assert 
        Assert.NotNull(sut);
        Assert.Equal("test2", actual);
    }

    internal class GenericClass<T> where T : class
    {
        public virtual T Passthrough(T arg) => arg;
    }
    
    internal interface IGenericInterface<T> where T : class
    {
        T Passthrough(T arg) => arg;
        
        T GenericParameter { get; set; }

        T this[string key] { get; set; }

        event EventHandler<T> GenericEvent;
    }
}