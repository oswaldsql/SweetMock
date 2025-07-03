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
}
