#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SweetMock
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    [System.CodeDom.Compiler.GeneratedCode("SweetMock","{{SweetMockVersion}}")]
    internal class MockAttribute<T> : Attribute { }

    /// <summary>
    /// Specifies that a mock should be using a specific custom implementation.
    /// </summary>
    /// <typeparam name="T">Type of class to mock.</typeparam>
    /// <typeparam name="TImplementation">Concrete type of use for mocking.</typeparam>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    [System.CodeDom.Compiler.GeneratedCode("SweetMock","{{SweetMockVersion}}")]
    internal class MockAttribute<T,TImplementation> : Attribute where TImplementation : T { }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    [System.CodeDom.Compiler.GeneratedCode("SweetMock","{{SweetMockVersion}}")]
    internal class FixtureAttribute<T> : Attribute { }
}
