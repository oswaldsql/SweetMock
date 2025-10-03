namespace SweetMock;

using System;

/// <summary>
/// Instructs SweetMock to create a fixture for a specific class.
/// </summary>
/// <typeparam name="T">The type to create a fixture for.</typeparam>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class FixtureAttribute<T>(string? fixtureName = null) : Attribute where T : class
{
    internal string FixtureName { get; } = fixtureName ?? typeof(T).Name;
}