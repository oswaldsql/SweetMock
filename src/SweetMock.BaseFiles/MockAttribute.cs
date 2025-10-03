namespace SweetMock;

using System;

/// <summary>
/// Instructs SweetMock to create a mock for a specific interface or class.
/// </summary>
/// <typeparam name="T">The type to create a mock based on.</typeparam>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class MockAttribute<T>(string? mockName = null) : Attribute
{
    public string? MockName { get; } = mockName ?? typeof(T).Name;
}

/// <summary>
/// Specifies that a mock should use a specific custom implementation.
/// </summary>
/// <typeparam name="T">Type of class to mock.</typeparam>
/// <typeparam name="TImplementation">Concrete type of use for mocking.</typeparam>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class MockAttribute<T, TImplementation> : Attribute
    where TImplementation : MockBase<T>, new()
;
