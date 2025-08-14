#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SweetMock
{
    /// <summary>
    /// Instructs SweetMock to create a mock for a specific interface or class.
    /// </summary>
    /// <typeparam name="T">The type to create a mock based on.</typeparam>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    [System.CodeDom.Compiler.GeneratedCode("SweetMock","{{SweetMockVersion}}")]
    internal class MockAttribute<T> : Attribute { }

    /// <summary>
    /// Specifies that a mock should use a specific custom implementation.
    /// </summary>
    /// <typeparam name="T">Type of class to mock.</typeparam>
    /// <typeparam name="TImplementation">Concrete type of use for mocking.</typeparam>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    [System.CodeDom.Compiler.GeneratedCode("SweetMock", "{{SweetMockVersion}}")]
    internal class MockAttribute<T, TImplementation> : Attribute;

    /// <summary>
    /// Instructs SweetMock to create a fixture for a specific class.
    /// </summary>
    /// <typeparam name="T">The type to create a fixture for.</typeparam>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    [System.CodeDom.Compiler.GeneratedCode("SweetMock","{{SweetMockVersion}}")]
    internal class FixtureAttribute<T> : Attribute where T : class { }
}
