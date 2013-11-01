using System;
using System.Linq;
using System.Threading;
using Connect;
using ContainerAgent;
using DataProvider.Loaders.History;
using DataProvider.Loaders.History.Data;
using DataProvider.Objects;
using NUnit.Framework;

namespace DataProviderTest {
    [TestFixture]
    public class HistoryTest {
        [TestCase]
        public void TestCreateAdx() {
            var container = Agent.Factory();
            var hst = container.GetInstance<IHistory>();
            var cnn = container.GetInstance<IConnection>();

            if (!cnn.ConnectAndWait(10))
                Assert.Inconclusive();
            
            hst.AppendField(HistoryField.Bid)
               .AppendField(HistoryField.Ask)
               .WithFeed("IDN")
               .WithNumRecords(30)
               .WithHistory(historyContainer => {
                   Console.WriteLine("Got it");
                   Console.WriteLine("Rics: {0}; Dates: {1}; Fields: {2}", 
                       historyContainer.Slice1().Count(), 
                       historyContainer.Slice2().Count(), 
                       historyContainer.Slice3().Count());
               })
               .Subscribe("GAZP.MM")
               .WithErrorCallback(Console.WriteLine)
               .WithTimeoutCallback(() => Console.WriteLine("Timeout!"))
               .WithTimeout(TimeSpan.FromSeconds(5))
               .Request();
            
            // todo no error on invalid ric or source
            // todo no error on inner exception

            Thread.Sleep(TimeSpan.FromSeconds(5));

            Console.WriteLine("== END ==");
        }

        [TestCase]
        public void TestCreateTsi() {
            var container = Agent.Factory();

            container.EjectAllInstancesOf<IHistoryRequest>();
            container.Configure(c => c.For<IHistoryRequest>().Use<TsiHistoryRequest>());

            container.EjectAllInstancesOf<IEikonObjects>();
            container.Configure(c => c.For<IEikonObjects>().Use<EikonObjectsPlVba>());


            var hst = container.GetInstance<IHistory>();

            var cnn = container.GetInstance<IConnection>();

            if (!cnn.ConnectAndWait(10))
                Assert.Inconclusive();

            hst.AppendField(HistoryField.Bid)
               .AppendField(HistoryField.Ask)
               .WithFeed("IDN")
               .WithNumRecords(30)
               .WithHistory(historyContainer => Console.WriteLine("Got it"))
               .Subscribe("GAZP.MM")
               .WithTimeoutCallback(() => Console.WriteLine("Timeout!"))
               .WithTimeout(TimeSpan.FromSeconds(5))
               .Request();

            Thread.Sleep(TimeSpan.FromSeconds(5));

            Console.WriteLine("== END ==");
        }
    }
}
