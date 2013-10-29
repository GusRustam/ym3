using System;
using System.Linq;
using System.Threading;
using Connect;
using ContainerAgent;
using DataProvider.Loaders.Realtime;
using DataProvider.Loaders.Realtime.Data;
using DataProvider.Loaders.Status;
using NUnit.Framework;
using Toolbox;

namespace DataProviderTest {
    [TestFixture]
    public class FieldsTest {
        private int _onImages;
        private int _onTimeouts;

        [SetUp]
        public void SetUp() {
            ClearCounters();
        }

        [TearDown]
        public void TearDown() {
            ClearCounters();
        }

        private void ClearCounters() {
            _onImages = 0;
            _onTimeouts = 0;
        }

        private void OnTimeout() {
            Console.WriteLine("OnTimeout()");
            _onTimeouts++;
        }

        private void OnImage(ISnapshot snapshot) {
            Console.WriteLine("OnImage()");
            Console.WriteLine("Status = {0}", snapshot.SourceStatus);
            foreach (var item in snapshot.Data) {
                Console.WriteLine(" -> ric {0}, status {1}", item.Ric, item.Status);
                foreach (var field in item.Fields) 
                    Console.WriteLine(" -> -> field {0} status {1} value {2}", field.Name, field.Status, field.Value);
            }

            _onImages++;
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
            var sub1 = subscriptionSetup1.DataRequest();
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
            var s1 = sS1.SnapshotReuqest();
            s1 = s1.WithCallback(OnImage);
            var t1 = s1.WithTimeout(OnTimeout);
            t1.Request();    

            var sS2 = x2.Subscribe(); // todo works both with x1 and x2
            sS2 = sS2.WithFields("BID", "ASK");
            var s2 = sS2.SnapshotReuqest();
            s2 = s2.WithCallback(OnImage);
            var t2 = s2.WithTimeout(OnTimeout);
            t2.Request();    

            Thread.Sleep(TimeSpan.FromSeconds(10));

            Assert.AreEqual(_onImages, 2);
        }

        [TestCase]
        public void SnapshotTimeout() {
            var factory = Agent.Factory();
            var conn = factory.GetInstance<IConnection>();

            if (!conn.ConnectAndWait(10))
                Assert.Inconclusive();

            var x = factory.GetInstance<IRealtime>();
            x = x.WithFeed("IDN");
            x = x.WithRics("GAZP.MM");

            var subscriptionSetup = x.Subscribe();
            subscriptionSetup = subscriptionSetup.WithFields("BID", "ASK");


            var subscription = subscriptionSetup.SnapshotReuqest(TimeSpan.FromMilliseconds(1));
            subscription = subscription.WithCallback(OnImage);
            var tm = subscription.WithTimeout(OnTimeout);
            tm.Request();

            Thread.Sleep(TimeSpan.FromSeconds(1));

            Assert.AreEqual(_onTimeouts, 1);
        }

        [TestCase]
        public void OnUpdateValidFeed() {
            var updates = 0;
            var sub1 = UpdateCall(() => updates++, "IDN", "EUR=", "GAZP.MM");

            Thread.Sleep(TimeSpan.FromSeconds(5));

            sub1.Stop();
            sub1.Close();
            Assert.GreaterOrEqual(updates, 1);
        }

        [TestCase]
        public void OnUpdateInvalidFeed() {
            var updates = 0;
            var sub1 = UpdateCall(() => {
                updates++; 
                
            }, "IDN1111", "EUR=", "GAZP.MM");

            Thread.Sleep(TimeSpan.FromSeconds(5));

            sub1.Stop();
            sub1.Close();
            Assert.AreEqual(updates, 0);
        }

        private static ISubscription UpdateCall(Action cb, string feed, params string[] rics) {
            var factory = Agent.Factory();
            var conn = factory.GetInstance<IConnection>();

            if (!conn.ConnectAndWait(10))
                Assert.Inconclusive();

            var x1 = factory.GetInstance<IRealtime>();
            x1 = x1.WithFeed(feed);
            x1 = x1.WithRics(rics);
            var subscriptionSetup1 = x1.Subscribe();
            subscriptionSetup1 = subscriptionSetup1.WithFields("BID", "ASK");
            subscriptionSetup1 = subscriptionSetup1.WithMode("TYPE:STRING");
            var sub1 = subscriptionSetup1.DataRequest();

            sub1 = sub1.WithCallback(upd => {
                cb();
                var data = upd.Data.ToSomeArray();
                Console.WriteLine("Update with source status {0} and list status {1}", upd.SourceStatus, upd.ListStatus);

                if (data.Any())
                    Console.WriteLine("Got update on ric {0}", data.First().Ric);
                else
                    Console.WriteLine("No rics :(");
            });
            sub1 = sub1.WithStatus((feedName, status, listStatus) =>
                Console.WriteLine("Feed {0} -> feed status {1}, list status {2}", feedName, status, listStatus));
            sub1.Start(RunMode.OnUpdate);
            return sub1;
        }

        [TestCase]
        public void OnTime() {
            // todo develop some version with invalid feeds and rics
            // todo I tried to turn the network off during the test but it didn't change anything
            var factory = Agent.Factory();
            var conn = factory.GetInstance<IConnection>();

            if (!conn.ConnectAndWait(10))
                Assert.Inconclusive();

            var x1 = factory.GetInstance<IRealtime>();
            x1 = x1.WithFeed("IDN");
            x1 = x1.WithRics("GAZP.MM", "EUR=");
            var sS1 = x1.Subscribe();
            sS1 = sS1.WithFields("BID", "ASK");
            sS1 = sS1.WithMode("TYPE:STRING");
            sS1 = sS1.WithFrq(TimeSpan.FromSeconds(1));
            var sub1 = sS1.DataRequest();
            var times = 0;
            sub1 = sub1.WithCallback(snapshot => {
                Console.WriteLine("Got snapshot. Feed status is {0}, list status is {1}", snapshot.SourceStatus, snapshot.ListStatus);
                foreach (var row in snapshot.Data) {
                    Console.WriteLine(" -> got ric {0} status {1}", row.Ric, row.Status);
                    foreach (var field in row.Fields) 
                        Console.WriteLine(" -> -> got field {0} status {1} value {2}", field.Name, field.Status, field.Value);
                }
                times++;
            });
            sub1.Start(RunMode.OnTime);

            Thread.Sleep(TimeSpan.FromSeconds(10));

            sub1.Stop();
            sub1.Close();
            Assert.GreaterOrEqual(times, 9);
        }

        private int _images;
        private int _rics;
        private int _totalFields;
        private ISourceStatus _status;

        private int _fields;
        private int _delayedRics;
        private int _okRics;
        private int _unknownRics;

        private void GetSnaphot(string feed) {
            var factory = Agent.Factory();
            var conn = factory.GetInstance<IConnection>();

            _images = 0;
            _rics = 0;
            _totalFields = 0;

            if (!conn.ConnectAndWait(10))
                Assert.Inconclusive();

            var x1 = factory.GetInstance<IRealtime>();
            x1 = x1.WithFeed(feed);
            x1 = x1.WithRics("GAZP.MM", "RUB=", "RU25YT=RR", "Babushka");
            var t1 = x1.Subscribe();
            t1 = t1.WithFields("BID", "ASK");
            var q1 = t1.SnapshotReuqest(TimeSpan.FromSeconds(5));

            q1 = q1.WithCallback(snapshot => {
                _images++;
                _status = snapshot.SourceStatus;
                Console.WriteLine("Got image; status = {0}", snapshot.SourceStatus);
                foreach (var item in snapshot.Data) {
                    _rics++;
                    Console.WriteLine(" -> ric {0}, status {1}", item.Ric, item.Status);
                    foreach (var field in item.Fields) {
                        _totalFields++;
                        Console.WriteLine(" -> -> field {0} status {1} value {2}", field.Name, field.Status, field.Value);
                    }
                }
            });
            var p1 = q1.WithTimeout(OnTimeout);
            p1.Request();

            Console.WriteLine("========= Wait start =============");
            Thread.Sleep(TimeSpan.FromSeconds(5));
            Console.WriteLine("========== Wait end ==============");
        }

        private void GetFields(string feed, params string[] rics) {
            var factory = Agent.Factory();
            var conn = factory.GetInstance<IConnection>();

            if (!conn.ConnectAndWait(10))
                Assert.Inconclusive();

            _fields = 0;
            _delayedRics = 0;
            _okRics = 0;
            _unknownRics = 0;

            var x1 = factory.GetInstance<IRealtime>();
            x1 = x1.WithFeed(feed);
            x1 = x1.WithRics(rics);

            var t1 = x1.Subscribe();
            var q1 = t1.FieldsRequest(TimeSpan.FromSeconds(5));
            q1 = q1.WithFields(ricsFields => {
                _fields++;
                if (ricsFields.SourceStatus != SourceStatus.Up) {
                    Console.WriteLine("Source down, no data maaan");
                } else {
                    var dictionary = ricsFields.Data;
                    foreach (var key in dictionary.Keys) {
                        if (dictionary[key].Status == ItemStatus.Delayed)
                            _delayedRics++;
                        else if (dictionary[key].Status == ItemStatus.Ok)
                            _okRics++;
                        else if (dictionary[key].Status == ItemStatus.UnknownOrInvalid)
                            _unknownRics++;

                        Console.WriteLine("{0} => {3} status, {1} fields, i.e. \n {2}",
                            key,
                            dictionary[key].Fields.Count(),
                            string.Join(", ", dictionary[key].Fields),
                            dictionary[key].Status);
                    }
                }
            });
            var p1 = q1.WithTimeout(OnTimeout);
            p1.Request();
            Console.WriteLine("======================");
            Thread.Sleep(TimeSpan.FromSeconds(10));
            Console.WriteLine("======================");
        }

        [TestCase]
        public void LoadFields() {
            GetFields("IDN", "GAZP.MM", "RUB=", "RU25YT=RR", "Babushka");
            Assert.AreEqual(_fields, 1);
            Assert.AreEqual(_delayedRics, 1);
            Assert.AreEqual(_okRics, 2);
            Assert.AreEqual(_unknownRics, 1);
        }

        [TestCase]
        public void LoadFieldsFromBadFeed() {
            GetFields("IDSSDN", "GAZP.MM", "RUB=", "RU25YT=RR", "Babushka");
            Assert.AreEqual(_fields, 1);
            Assert.AreEqual(_delayedRics, 0);
            Assert.AreEqual(_okRics, 0);
            Assert.AreEqual(_unknownRics, 0);
        }

        [TestCase]
        public void LoadSnapshotValidFeed() {
            GetSnaphot("IDN");
            Assert.AreEqual(_images, 1);
            Assert.AreEqual(_rics, 4);
            Assert.AreEqual(_totalFields, 4 * 2);
            Assert.AreEqual(_status, SourceStatus.Up);
        }

        [TestCase]
        public void LoadSnapshotInvalidFeed() {
            GetSnaphot("ZZZ");
            Assert.AreEqual(_images, 1);
            Assert.AreEqual(_rics, 0);
            Assert.AreEqual(_totalFields, 0);
            Assert.AreEqual(_status, SourceStatus.Unknown);
        }
    }
}
