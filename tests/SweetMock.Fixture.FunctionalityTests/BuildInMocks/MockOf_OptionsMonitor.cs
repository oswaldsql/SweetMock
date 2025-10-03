namespace SweetMock.FixtureGenerator.FunctionalityTests.BuildInMocks;

using Microsoft.Extensions.Options;

//[Mock<IOptionsMonitor<string>, MockOf_OptionsMonitor<string>>]
internal class MockOf_OptionsMonitor<TOptions> where TOptions : class
{
    public MockOptions Options { get; set; } = new MockOptions(instanceName: typeof(TOptions).Name);

    public MockConfig Config { get; } = new MockConfig();

    internal IOptionsMonitor<TOptions> Value => Config.Value;

    public class MockConfig
    {
        public OptionsMonitor<TOptions> Value { get; private set; }

        private readonly OptionsCache<TOptions> cache = new();
        public MockConfig()
        {
            var factory = new OptionsFactory<TOptions>([], []);
            Value = new OptionsMonitor<TOptions>(factory, [], cache);

            try
            {
                var options = Activator.CreateInstance<TOptions>();
                this.cache.TryAdd(null, options);
            }
            catch
            {
                // ignored
            }
        }
        
        public MockConfig Set(TOptions options)
        {
            this.cache.TryAdd(null, options);
            return this;
        }

        public MockConfig Set(string name, TOptions options)
        {
            this.cache.TryAdd(name, options);
            return this;
        }

    }
}