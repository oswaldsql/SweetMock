#nullable enable

using global::System;
using global::Microsoft.Extensions.Options;
using global::SweetMock;

namespace Microsoft.Extensions.Options{
    [System.CodeDom.Compiler.GeneratedCode("SweetMock","{{SweetMockVersion}}")]
    internal class MockOf_IOptions<TOptions>() where TOptions : class{
        public MockOptions Options { get; set; } = new MockOptions() {InstanceName = typeof(TOptions).Name};
        public MockConfig Config { get; } = new MockConfig();
        internal IOptions<TOptions> Value{
            get => Config.Value ?? throw new ArgumentNullException(Options.InstanceName, $"'{Options.InstanceName}' must have a value before being used.");
        }
        public class MockConfig{
            public IOptions<TOptions>? Value { get; private set; }
            public MockConfig(){
                try{
                    var options = Activator.CreateInstance<TOptions>();
                    this.Value = new OptionsWrapper<TOptions>((TOptions)options);
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
