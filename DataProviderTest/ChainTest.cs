using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Connect;
using ContainerAgent;
using DataProvider.Loaders.Chain;
using NUnit.Framework;
using Toolbox.Async;

namespace DataProviderTest {
    [TestFixture]
    public class ChainTest {
        public class Counts {
            public int ChainRics;
            public int Errors;
            public int Timeouts;
            public int Cancels;
            public int RicsAtLeast = -1; // no less than
            public int RicsAtMost = -1; // no more than
            public int RicsExactly = -1; // exactly

            public override string ToString() {
                return string.Format("Chains: {0} / Errors: {1} / Timeouts: {2} / Cancels: {3} / Rics: from {4} to {5}, exactly {6}", 
                    ChainRics, Errors, Timeouts, Cancels, 
                    RicsAtLeast == -1 ? "??" : RicsAtLeast.ToString(CultureInfo.InvariantCulture),
                    RicsAtMost == -1 ? "??" : RicsAtMost.ToString(CultureInfo.InvariantCulture),
                    RicsExactly == -1 ? "??" : RicsExactly.ToString(CultureInfo.InvariantCulture));
            }

            public bool Equals(Counts other) {
                Console.WriteLine("Comparing:\n{0}\n{1}", this, other);
                return 
                    ChainRics == other.ChainRics && 
                    Errors == other.Errors && 
                    Timeouts == other.Timeouts && 
                    Cancels == other.Cancels &&
                    (RicsAtLeast < 0 || other.RicsExactly >= RicsAtLeast) &&
                    (RicsAtMost < 0 || other.RicsExactly <= RicsAtMost) &&
                    (RicsExactly < 0 || other.RicsExactly == RicsExactly);
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
                        ChainRics = 1,
                        Errors = 2, // 2 errors - one on ric, one on request
                        RicsExactly = 0,
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
                        RicsExactly = 0,
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
                        Cancels = 2, // one for chain ric, one for all request
                        ChainRics = 1,
                        Errors = 0,
                        RicsExactly = 0,
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
                        Cancels = 2, // Cancel of the ric and cancel of all request
                        ChainRics = 1,
                        Errors = 0,
                        RicsExactly = 0,
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
                        ChainRics = 2,
                        Errors = 0,
                        RicsExactly = 0,
                        Timeouts = 3 // one for request, two for each chain
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
                        ChainRics = 2,
                        Errors = 2,  // by the number of invalid rics. But not 3 - outer request is ok
                        RicsExactly = 0,
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
                        ChainRics = 2,
                        Errors = 1, 
                        RicsAtLeast = 1,
                        RicsAtMost = 100,
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

            var l = new Counts { RicsExactly = 0 };
            var chainss = new HashSet<string>();
            var babushka = chn
                .WithFeed(prms.Feed)
                .WithChain(data => {
                    if (data.Status == TimeoutStatus.Cancelled)
                        l.Cancels++;

                    if (data.Status == TimeoutStatus.Error)
                        l.Errors++;

                    if (data.Status == TimeoutStatus.Timeout)
                        l.Timeouts++;

                    foreach (var k in data.Records) {
                        chainss.Add(k.ChainRic);
                        var count = k.Rics.Count();
                        Console.WriteLine("Got data on chain {0}, {1} items", k.ChainRic, count);
                        l.RicsExactly += count;
                        l.ChainRics += 1;

                        if (k.Status == null) throw new InvalidDataException("Status");
                        
                        if (k.Status == TimeoutStatus.Cancelled)
                            l.Cancels++;
                        
                        if (k.Status == TimeoutStatus.Error)
                            l.Errors++;

                        if (k.Status == TimeoutStatus.Timeout)
                            l.Timeouts++;
                    }
                })
                .WithRics(prms.ChainRics);

            var req = babushka.Subscribe();
           
            req
                //.WithErrorCallback(exception => {
                //    l.Errors++;
                //    Console.WriteLine("Error!\n {0}", exception);
                //})
                //.WithTimeoutCallback(() => {
                //    l.Timeouts++;
                //    Console.WriteLine("Timeout!");
                //})
                //.WithCancelCallback(() => {
                //    l.Cancels++;
                //    Console.WriteLine("Cancelled!");
                //})
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
                .WithChain(data => Console.WriteLine("Got data, {0} items", data.Records[0].Rics.Count()))
                .Subscribe()
                //.WithCancelCallback(() => Console.WriteLine("Cancel"))
                //.WithErrorCallback(exception => Console.WriteLine(string.Format("Error {0}", exception)))
                //.WithTimeoutCallback(() => Console.WriteLine("Timeout :("))
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
