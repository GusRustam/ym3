using System;
using System.Linq;
using System.Threading;
using Connect;
using ContainerAgent;
using DataProvider.Loaders.History;
using DataProvider.Loaders.History.Data;
using NUnit.Framework;

namespace DataProviderTest {
    [TestFixture]
    public class HistoryTest {
        [TestCase("IDN", "GAZP.MM", 1, 3, 0)]
        [TestCase("QQQ", "GAZP.MM", 0, 0, 1)]
        [TestCase("IDN", "XSSD.WE", 0, 0, 1)]
        public void TestCreateAdx(string feed, string ric, int rics, int fields, int errors) {
            var container = Agent.Factory();
            var hst = container.GetInstance<IHistory>();
            var cnn = container.GetInstance<IConnection>();

            if (!cnn.ConnectAndWait(10))
                Assert.Inconclusive();
            var errorCount = 0;

            var ricCount = 0;
            var fieldCount = 0;
            hst.AppendField(HistoryField.Bid)
               .AppendField(HistoryField.Ask)
               .WithFeed(feed)
               .WithNumRecords(30)
               .WithHistory(historyContainer => {
                   Console.WriteLine("Got it");
                   ricCount = historyContainer.Slice1().Count();
                   fieldCount = historyContainer.Slice3().Count();
                   var dateCount = historyContainer.Slice2().Count();
                   Console.WriteLine("Rics: {0}; Dates: {1}; Fields: {2}", ricCount, dateCount, fieldCount);
               })
               .Subscribe(ric)
               .WithErrorCallback(exception => {
                   errorCount++;
                   Console.WriteLine(exception);
               })
               .WithTimeoutCallback(() => Console.WriteLine("Timeout!"))
               .WithTimeout(TimeSpan.FromSeconds(5))
               .Request();

            Thread.Sleep(TimeSpan.FromSeconds(5));

            Assert.AreEqual(errorCount, errors);
            Assert.AreEqual(ricCount, rics);
            Assert.AreEqual(fieldCount, fields);
            

            Console.WriteLine("== END ==");
        }

        //[TestCase]
        //public void TestCreateTsi() {
        //    var container = Agent.Factory();

        //    container.EjectAllInstancesOf<IHistoryRequest>();
        //    container.Configure(c => c.For<IHistoryRequest>().Use<TsiHistoryRequest>());

        //    container.EjectAllInstancesOf<IEikonObjects>();
        //    container.Configure(c => c.For<IEikonObjects>().Use<EikonObjectsPlVba>());


        //    var hst = container.GetInstance<IHistory>();

        //    var cnn = container.GetInstance<IConnection>();

        //    if (!cnn.ConnectAndWait(10))
        //        Assert.Inconclusive();

        //    hst.AppendField(HistoryField.Bid)
        //       .AppendField(HistoryField.Ask)
        //       .WithFeed("IDN")
        //       .WithNumRecords(30)
        //       .WithHistory(historyContainer => Console.WriteLine("Got it"))
        //       .Subscribe("GAZP.MM")
        //       .WithTimeoutCallback(() => Console.WriteLine("Timeout!"))
        //       .WithTimeout(TimeSpan.FromSeconds(5))
        //       .Request();

        //    Thread.Sleep(TimeSpan.FromSeconds(5));

        //    Console.WriteLine("== END ==");
        //}
    }
}
