using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Connect;
using ContainerAgent;
using DataProvider.Loaders.Chain;
using NUnit.Framework;

namespace DataProviderTest {
    [TestFixture]
    public class ChainTest {
        public struct Counts {
            public int ChainRics;
            public int Errors;
            public int Timeouts;
            public int Cancels;
            public int Rics; // no less than

            public override string ToString() {
                return string.Format("{0} / {1} / {2} / {3} / {4}", ChainRics, Errors, Timeouts, Cancels, Rics);
            }

            public bool Equals(Counts other) {
                return ChainRics == other.ChainRics && Errors == other.Errors && Timeouts == other.Timeouts && Cancels == other.Cancels && Rics <= other.Rics;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                return obj is Counts && Equals((Counts) obj);
            }
        }

        public struct Params {
            public double TestTimeout;
            public double RequestTimeout;
            public string[] ChainRics;
            public string Feed;
            public bool DoCancel;

            public override string ToString() {
                return string.Format("{0} / {1} / {2} / {3} / {4}", string.Join(",", ChainRics), Feed, DoCancel, TestTimeout, RequestTimeout);
            }
        }

        public class MyTestCases {
            public static IEnumerable TestCases {
                get {
                    // Valid Feed, valid Ric
                    yield return new TestCaseData(new Params {
                        ChainRics = new []{ "0#RUCORP=MM" },
                        DoCancel = false,
                        Feed = "IDN",
                        RequestTimeout = 5,
                        TestTimeout = 5
                    }).Returns(new Counts {
                        Cancels = 0,
                        ChainRics = 1,
                        Errors = 0,
                        Rics = 100,
                        Timeouts = 0
                    });

                    // Valid Feed, invalid Ric
                    yield return new TestCaseData(new Params {
                        ChainRics = new[] { "0#RUCORP--XX=MM" },
                        DoCancel = false,
                        Feed = "IDN",
                        RequestTimeout = 5,
                        TestTimeout = 6
                    }).Returns(new Counts {
                        Cancels = 0,
                        ChainRics = 0,
                        Errors = 1,
                        Rics = 0,
                        Timeouts = 0
                    });

                    // Inalid Feed
                    yield return new TestCaseData(new Params {
                        ChainRics = new[] { "0#RUCORP--XX=MM" },
                        DoCancel = false,
                        Feed = "Q",
                        RequestTimeout = 5,
                        TestTimeout = 6
                    }).Returns(new Counts {
                        Cancels = 0,
                        ChainRics = 0,
                        Errors = 0,
                        Rics = 0,
                        Timeouts = 1
                    });

                    // todo multiple chain rics
                }
            }
        }

        private static Counts LoadChain(Params prms) {
            var container = Agent.Factory();
            var chn = container.GetInstance<IChain>();
            var cnn = container.GetInstance<IConnection>();

            if (!cnn.ConnectAndWait(10))
                Assert.Inconclusive();

            var l = new Counts();
            var chainss = new HashSet<string>();
            var babushka = chn
                .WithFeed(prms.Feed)
                .WithChain(data => {
                    chainss.Add(data.ChainRic);
                    var count = data.Rics.Count();
                    Console.WriteLine("Got data on chain {0}, {1} items", data.ChainRic, count);
                    l.Rics = count;
                    l.ChainRics = chainss.Count();
                })
                .WithRics(prms.ChainRics);

            var req = babushka.Subscribe();
           
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

        [TestCase]
        public void SimpleChainTest() {
            var container = Agent.Factory();
            var chn = container.GetInstance<IChain>();
            var cnn = container.GetInstance<IConnection>();

            if (!cnn.ConnectAndWait(10))
                Assert.Inconclusive();
            
            chn.WithFeed("IDN")
                .WithRics("0#RUCORP=MM")
                .WithChain(data => Console.WriteLine("Got data, {0} items", data.Rics.Count()))
                .Subscribe()
                .WithCancelCallback(() => Console.WriteLine("Cancel"))
                .WithErrorCallback(exception => Console.WriteLine(string.Format("Error {0}", exception)))
                .WithTimeoutCallback(() => Console.WriteLine("Timeout :("))
                .WithTimeout(TimeSpan.FromSeconds(5))
                .Request();

            Thread.Sleep(TimeSpan.FromSeconds(5));

            Console.WriteLine("== END ==");
        }

        [Test]
        [TestCaseSource(typeof(MyTestCases), "TestCases")]
        public Counts Tests(Params prms) {
            return LoadChain(prms);
        }
    }

   
}
