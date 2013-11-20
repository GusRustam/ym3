using System;
using System.Threading;
using ContainerAgent;
using DataProvider.Loaders.Metadata;
using DataProvider.Loaders.Metadata.Data;
using Moq;
using NUnit.Framework;

// todo Convertibles, index-linked, MBS (am I crazy?), repo, bond futures
// todo Get the widest chain list possible, load all rics from these chains. Make sure all data loads and filters out
namespace UnitTesting {
    [TestFixture]
    public class MetadataTest {
        public class EmptyData : IMetadataItem {
        }

        [TestCase]
        public void MockTest() {
            var locker = new object();

            var container = Agent.Factory();

            // creating mocks
            var mockMetaRequest = new Mock<IMetadataRequest<EmptyData>>();
            var mockMetaFactory = new Mock<IMetaObjectFactory<EmptyData>>();
            var mockMetaSetup = new Mock<IRequestSetup<EmptyData>>();

            container.Inject(mockMetaSetup.Object);
            container.Inject(mockMetaRequest.Object);
            container.Inject(mockMetaFactory.Object);

            // setting up callback
            Action<IMetadataContainer<EmptyData>> callback = data => {
                Console.WriteLine("Received Callback, into lock 2");
                lock (locker) {
                    Console.WriteLine("Pulse 2");
                    Monitor.Pulse(locker);
                }
            };

            // setting up the factory
            mockMetaFactory
                .Setup(f => f.CreateSetup())
                .Returns(container.GetInstance<IRequestSetup<EmptyData>>);

            mockMetaFactory
                .Setup(f => f.CreateRequest(It.IsAny<IRequestSetup<EmptyData>>()))
                .Returns(container.GetInstance<IMetadataRequest<EmptyData>>);

            // setting up request
            mockMetaRequest
                .Setup(r => r.WithTimeout(It.IsAny<TimeSpan>()))
                .Returns(mockMetaRequest.Object);
            mockMetaRequest
                .Setup(r => r.Request())
                .Callback(
                    () => new Thread(() => {
                        Console.WriteLine("Data requested, thinking");
                        Thread.Sleep(TimeSpan.FromSeconds(3));
                        callback(null);
                    }).Start());

            // initializing and requesting
            var mtd = container.GetInstance<IMetadata<EmptyData>>();
            mtd = mtd.WithRics("XXX");
            mtd = mtd.OnFinished(callback);
            var req = mtd.Request();
            var tm = req.WithTimeout(TimeSpan.FromSeconds(5));
            tm.Request();

            Console.WriteLine("Now I will wait for callback, into lock 1");
            lock (locker) {
                Console.WriteLine("Pulse 1");
                Monitor.Pulse(locker);
                Console.WriteLine("Wait 1");
                if (!Monitor.Wait(locker, TimeSpan.FromSeconds(6)))
                    Assert.Fail("Timeout");
            }

            Console.WriteLine("Done");
        }
    }
}