namespace TestSandbox;

using MassTransit;
using MassTransit.TestFramework;
using NUnit.Framework;
using SweetMock;

[Fixture<TestFixture>]
[Mock<ConsumeContext<Payload>, MockOf_ConsumeContext<Payload>>]
[Mock<HttpClient>]
[Mock<HttpMessageHandler>]
public class UnitTest1
{
    [Test]
    public void ValidateThatHttpClientCanBeMocked()
    {
        Fixture.TestFixture(config =>
        {
            config.context.Value = new TestConsumeContext<string>("test");
            config.client.SendAsync(new HttpResponseMessage());
        });
    }

//    [Fact]
//    [Mock<ISlimLogger<UnitTest1>>]
//    public void Test1()
//    {
//        var mock = Mock.ISlimLogger<UnitTest1>();
//    }

//    public class SlimLogger2<T> : ISlimLogger<T> 
//    {
//        private SweetMock.CallLog? _sweetMockCallLog = new SweetMock.CallLog();
//        
//        void TestSandbox.UnitTest1.ISlimLogger<T>.Log<TState>(System.Func<TState, System.Exception?, string> formatter){
//            if(_sweetMockCallLog != null){
//                _sweetMockCallLog.Add("TestSandbox.UnitTest1.ISlimLogger<T>.Log<TState>(System.Func<TState, System.Exception?, string>)", SweetMock.Arguments.With("formatter", formatter));
//            }
//            this._Log.Invoke(formatter, typeof(TState));
//        }
//        private Config.DelegateFor_Log _Log {get;set;} = (System.Func<TState, System.Exception?, string> formatter, System.Type typeOf_TState) => throw new System.InvalidOperationException("The method 'Log' in 'ISlimLogger' is not explicitly mocked.") {Source = "TestSandbox.UnitTest1.ISlimLogger<T>.Log<TState>(System.Func<TState, System.Exception?, string>)"};
//        internal partial class Config{
//            public delegate void DelegateFor_Log(System.Func<TState, System.Exception?, string> formatter, System.Type typeOf_TState);
//            public Config Log(DelegateFor_Log call){
//                target._Log = call;
//                return this;
//            }
//        }
//    }
//    
//    public interface ISlimLogger<T>
//    {
//        void Log<TState>(Func<TState, Exception?, string> formatter);
//    }
}

public class TestFixture(ConsumeContext<string> context, HttpClient client)
{
    public ConsumeContext<string> Context { get; } = context;
    public HttpClient Client { get; } = client;
}

public class Payload
{
}

internal class MockOf_ConsumeContext<T> : MockBase<ConsumeContext<T>> where T : class;