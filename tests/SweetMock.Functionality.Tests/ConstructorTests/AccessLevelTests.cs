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