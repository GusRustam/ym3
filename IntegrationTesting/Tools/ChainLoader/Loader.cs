using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Connect;
using ContainerAgent;
using DataProvider.Loaders.Chain;
using IntegrationTesting.Data;
using NUnit.Framework;
using Toolbox.Async;

namespace IntegrationTesting.Tools.ChainLoader {
    public struct ChainParams {
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

    [TestFixture]
    public class Loader {
        [TestCase]
        public void LoadChain() {
            var container = Agent.Factory();
            var chn = container.GetInstance<IChain>();
            var cnn = container.GetInstance<IConnection>();

            var locker = new object();

            if (!cnn.ConnectAndWait(10))
                Assert.Inconclusive();

            var bondRics = new Dictionary<string, string>();
            var chainsToLoad = new List<string>(new[] {"0#BONDS", "0#EUROBONDS"});
            var loadedChains = new HashSet<string>();

            do {
                lock(locker) if (!chainsToLoad.Any()) break;
                Console.WriteLine("To load chains {0}", string.Join(",", chainsToLoad));
                chn.WithFeed("IDN")
                    .WithChain(data => {
                        try {
                            Console.WriteLine("Got data");

                            if (data.Status == TimeoutStatus.Ok) {
                                foreach (var record in data.Records) {
                                    Console.WriteLine("Parsing chain ric {0}, status {1}", record.ChainRic, record.Status);
                                    
                                    if (record.Status != TimeoutStatus.Ok)
                                        continue;

                                    var anyChain = true;
                                    foreach (var ric in record.Rics) {
                                        if (ric.StartsWith("0#")) {
                                            anyChain = false;
                                            
                                            // load moar chains
                                            if (loadedChains.Contains(ric)) {
                                                Console.WriteLine("I will not load chain {0} once again", ric);
                                            } else {
                                                Console.WriteLine("Chain {0} to be loaded", ric);
                                                chainsToLoad.Add(ric);
                                            }
                                        } else {
                                            // save the rics
                                            bondRics.Add(ric, record.ChainRic);
                                        }
                                    }
                                    if (!anyChain) {
                                        Console.WriteLine();
                                        chainsToLoad.Remove(record.ChainRic);
                                        loadedChains.Add(record.ChainRic);
                                    }
                                }
                            }
                        } catch (Exception e) {
                            Console.WriteLine(e);
                        } finally {
                            lock (locker)
                                Monitor.Pulse(locker);
                        }
                    })
                    .WithRics(chainsToLoad.ToArray()) // todo do not start more than 100 threads; todo store chain tree!
                    .Subscribe()
                    .Request();

                lock (locker) {
                    Monitor.Pulse(locker);
                    Monitor.Wait(locker);
                }
            } while (true);


            //return l;
        }
    }
}
