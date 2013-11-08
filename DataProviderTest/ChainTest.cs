using System;
using System.Linq;
using System.Threading;
using Connect;
using ContainerAgent;
using DataProvider.Loaders.Chain;
using NUnit.Framework;

namespace DataProviderTest {
    [TestFixture]
    public class ChainTest {
        [TestCase]
        public void SimpleChainTest() {
            var container = Agent.Factory();
            var chn = container.GetInstance<IChain>();
            var cnn = container.GetInstance<IConnection>();

            if (!cnn.ConnectAndWait(10))
                Assert.Inconclusive();

            
            chn.WithFeed("IDN")
                .WithRics("0#RUCORP=MM")
                .WIthChain(data => Console.WriteLine("Got data, {0} items", data.Rics.Count()))
                .Subscribe()
                .WithCancelCallback(() => Console.WriteLine("Cancel"))
                .WithErrorCallback(exception => Console.WriteLine(string.Format("Error {0}", exception)))
                .WithTimeoutCallback(() => Console.WriteLine("Timeout"))
                .WithTimeout(TimeSpan.FromSeconds(5))
                .Request();

            Thread.Sleep(TimeSpan.FromSeconds(5));

            Console.WriteLine("== END ==");
        }
    }
}
