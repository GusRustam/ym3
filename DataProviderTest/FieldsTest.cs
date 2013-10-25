using System;
using System.Linq;
using System.Threading;
using Connect;
using ContainerAgent;
using DataProvider.DataLoaders;
using DataProvider.DataLoaders.Status;
using NUnit.Framework;

namespace DataProviderTest {
    [TestFixture]
    public class FieldsTest {
        private int _onUpdates;
        private int _onImages;
        private int _onTimes;
        private int _onTimeouts;
        private int _onFields;

        [SetUp]
        public void SetUp() {
            ClearCounters();
        }

        [TearDown]
        public void TearDown() {
            ClearCounters();
        }

        private void ClearCounters() {
            _onUpdates = 0;
            _onImages = 0;
            _onTimes = 0;
            _onTimeouts = 0;
            _onFields = 0;
        }
        private void OnTimeout() {
            Console.WriteLine("OnTimeout()");
            _onTimeouts++;
        }

        private void OnFields(IRicsFields ricsFields) {
            Console.WriteLine("OnFields()");
            if (ricsFields.SourceStatus != SourceStatus.Up) {
                Console.WriteLine("Source down, no data maaan");
            } else {
                var dictionary = ricsFields.Data;
                foreach (var key in dictionary.Keys) {
                    Console.WriteLine("{0} => {3} status, {1} fields, i.e. \n {2}",
                        key,
                        dictionary[key].Fields.Count(),
                        string.Join(", ", dictionary[key].Fields),
                        dictionary[key].Status);
                }
            }
            _onFields++;
        }

        private  void OnUpdate(string name, object tag, IItemStatus status) {
            Console.WriteLine("OnUpdate({0}, {1}, {2})", name, tag, status);
            _onUpdates++;
        }

        private static void OnStatusChange(IListStatus status, ISourceStatus sourceStatus, IRunMode mode) {
            Console.WriteLine("OnStatusChange({0}, {1}, {2})", status, sourceStatus, mode);  
        }

        private void OnImage(IDataStatus status) {
            Console.WriteLine("OnImage({0})", status);
            _onImages++;
        }

        private void OnTimeHandler() {
            Console.WriteLine("OnTime()");
            _onTimes++;
        }

        [TestCase]
        public void CreateAndKill() {
            var factory = Agent.Factory();
            var conn = factory.GetInstance<IConnection>();

            if (!conn.ConnectAndWait(10))
                Assert.Inconclusive();

            var x1 = factory.GetInstance<IRealtime>();
            x1 = x1.WithFeed("IDN");
            x1 = x1.WithRics("GAZP.MM");
            var subscriptionSetup1 = x1.Subscribe();
            subscriptionSetup1 = subscriptionSetup1.WithFields("BID", "ASK");
            var sub1 = subscriptionSetup1.Create();
            sub1.Stop();
            sub1.Close();
        }

        [TestCase]
        public void TwoSubscriptions() {
            var factory = Agent.Factory();
            var conn = factory.GetInstance<IConnection>();
            if (!conn.ConnectAndWait(10))
                Assert.Inconclusive();

            var x1 = factory.GetInstance<IRealtime>();
            x1 = x1.WithFeed("IDN");
            x1 = x1.WithRics("GAZP.MM");
            var x2 = factory.GetInstance<IRealtime>();
            x2 = x2.WithFeed("IDN");
            x2 = x2.WithRics("GAZP.MM");


            var sS1 = x1.Subscribe();
            sS1 = sS1.WithFields("BID", "ASK");
            var s1 = sS1.ReuqestSnapshot();
            s1 = s1.WithCallback(OnImage);
            var t1 = s1.WithTimeout(OnTimeout);
            t1.Request();    

            var sS2 = x2.Subscribe(); // todo works both with x1 and x2
            sS2 = sS2.WithFields("BID", "ASK");
            var s2 = sS2.ReuqestSnapshot();
            s2 = s2.WithCallback(OnImage);
            var t2 = s2.WithTimeout(OnTimeout);
            t2.Request();    

            Thread.Sleep(TimeSpan.FromSeconds(5));

            Assert.AreEqual(_onImages, 2);
        }

        [TestCase]
        public void Snapshot() {
            var factory = Agent.Factory();
            var conn = factory.GetInstance<IConnection>();

            if (!conn.ConnectAndWait(10))
                Assert.Inconclusive();

            var x = factory.GetInstance<IRealtime>();
            x = x.WithFeed("IDN");
            x = x.WithRics("GAZP.MM");

            var subscriptionSetup = x.Subscribe();
            subscriptionSetup = subscriptionSetup.WithFields("BID", "ASK");


            var subscription = subscriptionSetup.ReuqestSnapshot(TimeSpan.FromSeconds(1));
            subscription = subscription.WithCallback(OnImage);
            var tm = subscription.WithTimeout(OnTimeout);
            tm.Request();

            Thread.Sleep(TimeSpan.FromSeconds(5));

            Assert.AreEqual(_onImages, 1);
        }

        [TestCase]
        public void OnTime() {
            var factory = Agent.Factory();
            var conn = factory.GetInstance<IConnection>();

            if (!conn.ConnectAndWait(10))
                Assert.Inconclusive();

            var x1 = factory.GetInstance<IRealtime>();
            x1 = x1.WithFeed("IDN");
            x1 = x1.WithRics("GAZP.MM");
            var subscriptionSetup1 = x1.Subscribe();
            subscriptionSetup1 = subscriptionSetup1.WithFields("BID", "ASK");
            subscriptionSetup1 = subscriptionSetup1.WithFrq(TimeSpan.FromSeconds(1));
            var sub1 = subscriptionSetup1.Create();
            sub1 = sub1.OnTime(OnTimeHandler);
            sub1.Start(RunMode.OnTime);

            Thread.Sleep(TimeSpan.FromSeconds(10));

            sub1.Stop();
            sub1.Close();
            Assert.AreEqual(_onTimes, 10);
        }

        [TestCase]
        public void LoadFields() {
            // todo test invalid feed => Shows all rics to be invalid, not quite good
            // todo play with feeding from excel and from here
            // todo try getting source list
            // 
            // todo test invalid ric -> ok
            var factory = Agent.Factory();
            var conn = factory.GetInstance<IConnection>();

            if (!conn.ConnectAndWait(10))
                Assert.Inconclusive();

            var x1 = factory.GetInstance<IRealtime>();
            x1 = x1.WithFeed("IDN");
            x1 = x1.WithRics("GAZP.MM", "RUB=", "RU25YT=RR", "Babushka");

            var t1 = x1.Subscribe();
            var q1 = t1.RequestFields(TimeSpan.FromSeconds(5));
            q1 = q1.WithFields(OnFields);
            var p1 = q1.WithTimeout(OnTimeout);
            p1.Request();
            Console.WriteLine("======================");
            Thread.Sleep(TimeSpan.FromSeconds(10));
            Console.WriteLine("======================");
            Assert.AreEqual(_onFields, 1);   
        }

        [TestCase]
        public void LoadRustam() {
            // todo test separately wrong ric and wrong IDN

            var factory = Agent.Factory();
            var conn = factory.GetInstance<IConnection>();

            if (!conn.ConnectAndWait(10))
                Assert.Inconclusive();

            var x1 = factory.GetInstance<IRealtime>();
            x1 = x1.WithFeed("IDN");
            x1 = x1.WithRics("RUSTAM");

            var t1 = x1.Subscribe();
            var q1 = t1.RequestFields(TimeSpan.FromSeconds(5));
            q1 = q1.WithFields(OnFields);
            var p1 = q1.WithTimeout(OnTimeout);
            p1.Request();
            Console.WriteLine("======================");
            Thread.Sleep(TimeSpan.FromSeconds(10));
            Console.WriteLine("======================");
            Assert.AreEqual(_onFields, 1);
        }

    }
}
