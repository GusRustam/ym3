using System;
using Connect;
using ContainerAgent;
using NUnit.Framework;

namespace ConnectionTest {
    [TestFixture]
    public class EikonConnectionTest {

        [TestCase]
        public void TestConnectionWithEmbeddedWaiter() {
            var factory = Agent.Factory();
            var x = factory.GetInstance<IConnection>();
            x.Connected += () => Console.WriteLine("OnConnected");
            x.Disconnected += () => Console.WriteLine("OnDisconnected");
            var res = x.ConnectAndWait(10);
            Console.WriteLine(res ? "Connected yeah" : "Failed to connect");
            Assert.True(res);
            res = x.DisconnectAndWait();
            Console.WriteLine(res ? "Disconnected yeah" : "Failed to disconnect");
            Assert.True(res);
        }

        [TestCase]
        public void ConnectDisconnectAndAgain() {
            var factory = Agent.Factory();
            var x = factory.GetInstance<IConnection>();
            x.Connected += () => Console.WriteLine("OnConnected");
            x.Disconnected += () => Console.WriteLine("OnDisconnected");
               
            var res = x.ConnectAndWait(10);
            Console.WriteLine(res ? "Connected yeah" : "Failed to connect");
            Assert.True(res);
            res = x.DisconnectAndWait();
            Console.WriteLine(res ? "Disconnected yeah" : "Failed to disconnect");
            Assert.True(res);

            //x = factory.GetInstance<IConnection>();
            res = x.ConnectAndWait(10);
            Console.WriteLine(res ? "Connected yeah" : "Failed to connect");
            Assert.True(res);
            res = x.DisconnectAndWait();
            Console.WriteLine(res ? "Disconnected yeah" : "Failed to disconnect");
            Assert.True(res);
        }

        [TestCase]
        public void AFailToConnectAndThenConnect() {
            var factory = Agent.Factory();
            var x = factory.GetInstance<IConnection>();
            x.Connected += () => Console.WriteLine("OnConnected");
            x.Disconnected += () => Console.WriteLine("OnDisconnected");

            var res = x.ConnectAndWait(1);
            Console.WriteLine(res ? "Connected yeah" : "Failed to connect");
            Assert.False(res);
            res = x.DisconnectAndWait();
            Console.WriteLine(res ? "Disconnected yeah" : "Failed to disconnect");
            Assert.True(res);

            res = x.ConnectAndWait(10);
            Console.WriteLine(res ? "Connected yeah" : "Failed to connect");
            Assert.True(res);
            res = x.DisconnectAndWait();
            Console.WriteLine(res ? "Disconnected yeah" : "Failed to disconnect");
            Assert.True(res);
        }
    }
}
