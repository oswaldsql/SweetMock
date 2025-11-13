// ReSharper disable RedundantNullableDirective
// ReSharper disable ArrangeNamespaceBody
// ReSharper disable RedundantNameQualifier
// ReSharper disable RedundantBaseQualifier
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
#nullable enable

namespace Microsoft.Extensions.Options{
    [System.CodeDom.Compiler.GeneratedCode("SweetMock","{{SweetMockVersion}}")]
    internal class MockOf_IOptions<TOptions> where TOptions : class
    {
        public global::SweetMock.MockOptions Options { get; set; } = new(instanceName: typeof(TOptions).Name);
        public MockConfig Config { get; } = new();
        internal IOptions<TOptions> Value => this.Config.Value ?? throw new global::System.ArgumentNullException(this.Options.InstanceName, $"'{this.Options.InstanceName}' must have a value before being used.");

        public class MockConfig{
            public IOptions<TOptions>? Value { get; private set; }
            public MockConfig(){
                try{
                    var options = global::System.Activator.CreateInstance<TOptions>();
                    this.Value = new OptionsWrapper<TOptions>(options);
                }
                catch{
                    // ignored
                }
            }
            public MockConfig Set(TOptions options){
                this.Value = new OptionsWrapper<TOptions>(options);
                return this;
            }
        }
    }
}
