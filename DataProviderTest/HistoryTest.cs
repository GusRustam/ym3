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
        private struct Counts {
            public int Fields;
            public int Rics;
            public int Dates;
            public int Errors;
            public int Timeouts;
            public int Cancels;
        }
        private struct Params {
            public double TestTimeout;
            public double RequestTimeout;
            public string[] Rics;
            public string Feed;
            public IHistoryField[] Fields;
            public bool DoCancel;
        }

        [TestCase("IDN", "GAZP.MM", 1, 3, 0)]
        [TestCase("QQQ", "GAZP.MM", 0, 0, 1)]
        [TestCase("IDN", "XSSD.WE", 0, 0, 1)]
        public void TestLoadHistory(string feed, string ric, int rics, int fields, int errors) {
            var l = LoadHist(new Params {
                Feed = feed,
                Rics = new[] {ric},
                Fields = new[] {HistoryField.Bid, HistoryField.Ask},
                TestTimeout = 5,
                RequestTimeout = 5,
                DoCancel = false
            });

            Assert.AreEqual(l.Errors, errors);
            Assert.AreEqual(l.Rics, rics);
            Assert.AreEqual(l.Fields, fields);

        }

        [TestCase(new[] { "GAZP.MM", "LKOH.MM" }, 1)]
        [TestCase(new[] { "GAZP.MM" }, 0)]
        public void TestTimeout(string[] rics, int dummy) {

            var l = LoadHist(new Params {
                Feed = "IDN",
                Rics = rics,
                Fields = new[] {HistoryField.Bid, HistoryField.Ask},
                TestTimeout = 5,
                RequestTimeout = 0.5,
                DoCancel = false
            });

            Assert.AreEqual(l.Errors, 0);
            Assert.AreEqual(l.Rics, 0);
            Assert.AreEqual(l.Fields, 0);
            Assert.AreEqual(l.Cancels, 0);
            Assert.AreEqual(l.Timeouts, 1);
        }

        [TestCase(new[] { "GAZP.MM", "LKOH.MM" }, 1)]
        [TestCase(new[] { "GAZP.MM" }, 0)]
        public void TestCancel(string[] rics, int dummy) {

            var l = LoadHist(new Params {
                Feed = "IDN",
                Rics = rics,
                Fields = new[] { HistoryField.Bid, HistoryField.Ask },
                TestTimeout = 0.5,
                RequestTimeout = 5,
                DoCancel = true
            });

            Assert.AreEqual(l.Errors, 0);
            Assert.AreEqual(l.Rics, 0);
            Assert.AreEqual(l.Fields, 0);
            Assert.AreEqual(l.Cancels, 1);
        }



        [TestCase("IDN", new[] { "GAZP.MM", "LKOH.MM" }, 2, 3, 0)]
        [TestCase("IDN", new[] { "XXDFDF", "LKOH.MM" }, 1, 3, 0)]
        [TestCase("IDN", new[] { "GAZP.MMasddas", "LKOH.asdasMM" }, 0, 0, 0)]
        [TestCase("IDNXXX", new[] { "GAZP.MM", "LKOH.MM" }, 0, 0, 0)]
        public void TestLoadBunch(string feed, string[] what, int rics, int fields, int errors) {
            var l = LoadHist(new Params {
                Feed = feed,
                Rics = what,
                Fields = new[] { HistoryField.Bid, HistoryField.Ask },
                TestTimeout = 5,
                RequestTimeout = 5,
                DoCancel = false
            });


            Assert.AreEqual(l.Errors, errors);
            Assert.AreEqual(l.Rics, rics);
            Assert.AreEqual(l.Fields, fields);
        }

        private static Counts LoadHist(Params prms) {
            var container = Agent.Factory();
            var hst = container.GetInstance<IHistory>();
            var cnn = container.GetInstance<IConnection>();

            if (!cnn.ConnectAndWait(10))
                Assert.Inconclusive();

            var l = new Counts();

            var babushka = hst
                .AppendFields(prms.Fields)
                .WithFeed(prms.Feed)
                .WithNumRecords(30)
                .WithHistory(historyContainer => {
                    Console.WriteLine("History!");
                    l.Rics = historyContainer.Slice1().Count();
                    l.Fields = historyContainer.Slice3().Count();
                    l.Dates = historyContainer.Slice2().Count();
                    Console.WriteLine("Rics: {0}; Dates: {1}; Fields: {2}", l.Rics, l.Dates, l.Fields);
                });


            var req = prms.Rics.Count() == 1 ?
                babushka.Subscribe(prms.Rics[0]) :
                babushka.Subscribe(prms.Rics);

            req.WithErrorCallback(exception => {
                l.Errors++;
                Console.WriteLine("Error!\n {0}", exception);
            })
                .WithTimeoutCallback(() => {
                    l.Timeouts++;
                    Console.WriteLine("Timeout!");
                })
                .WithCancelCallback(() => {
                    l.Cancels++;
                    Console.WriteLine("Cancelled!");
                })
                .WithTimeout(TimeSpan.FromSeconds(prms.RequestTimeout))
                .Request();

            Thread.Sleep(TimeSpan.FromSeconds(prms.TestTimeout));
            if (prms.DoCancel)
                req.Cancel();

            Console.WriteLine("== END ==");
            return l;
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
