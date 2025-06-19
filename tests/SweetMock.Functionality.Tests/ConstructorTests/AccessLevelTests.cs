// ReSharper disable EmptyConstructor
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local

namespace Test.ConstructorTests;

using System.Reflection;

public class AccessLevelTests
{
    [Fact]
    [Mock<AccessLevelTestClass>]
    public void OnlyPublicAndProtectedCtorAreExposed()
    {
        // Arrange

        // ACT
        var actual = typeof(MockOf_AccessLevelTestClass).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var info in actual)
        {
            Console.WriteLine(info);
        }
        
        // Assert
        Assert.Equal(3, actual.Length);
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