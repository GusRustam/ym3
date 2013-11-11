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
            public int RicsAtLeast; // no less than

            public override string ToString() {
                return string.Format("Chains: {0} / Errors: {1} / Timeouts: {2} / Cancels: {3} / Rics: {4}", ChainRics, Errors, Timeouts, Cancels, RicsAtLeast);
            }

            public bool Equals(Counts other) {
                return ChainRics == other.ChainRics && Errors == other.Errors && Timeouts == other.Timeouts && Cancels == other.Cancels && RicsAtLeast <= other.RicsAtLeast;
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
            public string Description;

            public override string ToString() {
                return Description;
            }
        }

        public class MyTestCases {
            public static IEnumerable TestCases {
                get {
                    yield return new TestCaseData(new Params {
                        Description = "Valid Feed, valid Ric",
                        ChainRics = new []{ "0#RUCORP=MM" },
                        DoCancel = false,
                        Feed = "IDN",
                        RequestTimeout = 5,
                        TestTimeout = 5
                    }).Returns(new Counts {
                        Cancels = 0,
                        ChainRics = 1,
                        Errors = 0,
                        RicsAtLeast = 100,
                        Timeouts = 0
                    });

                    yield return new TestCaseData(new Params {
                        Description = "Valid Feed, invalid Ric",
                        ChainRics = new[] { "0#RUCORP--XX=MM" },
                        DoCancel = false,
                        Feed = "IDN",
                        RequestTimeout = 5,
                        TestTimeout = 6
                    }).Returns(new Counts {
                        Cancels = 0,
                        ChainRics = 0,
                        Errors = 1,
                        RicsAtLeast = 0,
                        Timeouts = 0
                    });

                    yield return new TestCaseData(new Params {
                        Description = "Inalid Feed",
                        ChainRics = new[] { "0#RUCORP--XX=MM" },
                        DoCancel = false,
                        Feed = "Q",
                        RequestTimeout = 5,
                        TestTimeout = 6
                    }).Returns(new Counts {
                        Cancels = 0,
                        ChainRics = 0,
                        Errors = 0,
                        RicsAtLeast = 0,
                        Timeouts = 1
                    });

                    yield return new TestCaseData(new Params {
                        Description = "Inalid Feed, cancellation",
                        ChainRics = new[] { "0#RUCORP--XX=MM" },
                        DoCancel = true,
                        Feed = "Q",
                        RequestTimeout = 5,
                        TestTimeout = 1
                    }).Returns(new Counts {
                        Cancels = 1,
                        ChainRics = 0,
                        Errors = 0,
                        RicsAtLeast = 0,
                        Timeouts = 0
                    });

                    yield return new TestCaseData(new Params {
                        Description = "Valid Feed, cancellation",
                        ChainRics = new[] { "0#RUCORP--XX=MM" },
                        DoCancel = true,
                        Feed = "Q",
                        RequestTimeout = 5,
                        TestTimeout = 1
                    }).Returns(new Counts {
                        Cancels = 1,
                        ChainRics = 0,
                        Errors = 0,
                        RicsAtLeast = 0,
                        Timeouts = 0
                    });

                    yield return new TestCaseData(new Params {
                        Description = "Many Rics, Valid Feed, valid Ric",
                        ChainRics = new[] { "0#RUCORP=MM", "0#RUTSY=MM" },
                        DoCancel = false,
                        Feed = "IDN",
                        RequestTimeout = 5,
                        TestTimeout = 5
                    }).Returns(new Counts {
                        Cancels = 0,
                        ChainRics = 2,
                        Errors = 0,
                        RicsAtLeast = 100,
                        Timeouts = 0
                    });

                    yield return new TestCaseData(new Params {
                        Description = "Many Rics, Invalid Feed",
                        ChainRics = new[] { "0#RUCORP=MM", "0#RUTSY=MM" },
                        DoCancel = false,
                        Feed = "QQQQQQQQ",
                        RequestTimeout = 5,
                        TestTimeout = 6
                    }).Returns(new Counts {
                        Cancels = 0,
                        ChainRics = 0,
                        Errors = 0,
                        RicsAtLeast = 0,
                        Timeouts = 1 // todo I don't really like that I have different outcomes on erroneous data... Here I will have no errors at all, but a timeout message.
                    });

                    yield return new TestCaseData(new Params {
                        Description = "Many Rics, Valid Feed, Invalid Rics",
                        ChainRics = new[] { "0#RUCORP=MM11111", "0#RUTSY=MM11111" },
                        DoCancel = false,
                        Feed = "IDN",
                        RequestTimeout = 5,
                        TestTimeout = 5
                    }).Returns(new Counts {
                        Cancels = 0,
                        ChainRics = 0,
                        Errors = 0, // todo I don't really like that I have different outcomes on erroneous data... Here I will have no errors at all, but empty response.
                        RicsAtLeast = 0,
                        Timeouts = 0
                    });

                    yield return new TestCaseData(new Params {
                        Description = "Many Rics, Valid Feed, Valid and Invalid Rics",
                        ChainRics = new[] { "0#RUCORP=MM11111", "0#RUTSY=MM" },
                        DoCancel = false,
                        Feed = "IDN",
                        RequestTimeout = 5,
                        TestTimeout = 6
                    }).Returns(new Counts {
                        Cancels = 0,
                        ChainRics = 1,
                        Errors = 0, // todo I don't really like that I have different outcomes on erroneous data... Here I will have no errors at all, but partially empty response.
                        RicsAtLeast = 1,
                        Timeouts = 0
                    });

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
                    foreach (var k in data.Data.Keys) {
                        chainss.Add(k);
                        var count = data.Data[k].Count();
                        Console.WriteLine("Got data on chain {0}, {1} items", k, count);
                        l.RicsAtLeast += count;
                        l.ChainRics += 1;
                        
                    }
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
                .WithChain(data => Console.WriteLine("Got data, {0} items", data.Data.Keys.First().Count()))
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
