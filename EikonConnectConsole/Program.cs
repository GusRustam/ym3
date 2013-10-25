using System;
using System.Threading;
using Connect;
using EikonDesktopDataAPILib;
using LoggingFacility;
using LoggingFacility.Loggers;
using Task = System.Threading.Tasks.Task;

namespace EikonConnectConsole {
    class Program {
        public static ILogger Logger { get; set; }

        private static bool ConnectAndWait(IConnection cnn, int seconds) {
            var cancellation = new CancellationTokenSource();

            // this is an anonymous event handler that has to be attached to 
            // and detached from Connected event. When Connected event occurs,
            // it simply stops the waiter
            Action cn = cancellation.Cancel;

            // this is a new thread which only task is to wait until it is cancelled
            var task = Task.Factory.StartNew(
                () => cancellation.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(2 * seconds)),
                cancellation.Token);

            // adding handler
            cnn.Connected += cn;

            // asking for connection in no more than given time
            cnn.Connect(TimeSpan.FromSeconds(seconds));

            bool connected;
            try {
                // starting connection waiter thread
                task.Wait(cancellation.Token);
                connected = cancellation.IsCancellationRequested;
            } catch (OperationCanceledException) {
                // very interesting! is sometimes reports OperationCanceledException, which shouldn't be the case
                Logger.Log(Level.Info, "OperationCanceledException is okay");
                try {
                    var bond = cnn.Sdk.CreateAdxBondModule();
                    connected = bond != null;
                } catch (Exception) {
                    Logger.Log(Level.Info, "failed to create bond is not okay");
                    connected = false;
                }
            }

            // when it's finished (either by cancel or by timeout), remove Connected handler
            cnn.Connected -= cn;

            // if cancellation was requested, this means that Connected event had 
            // happened, otherwise no connection was established
            return connected;
        }

        private static bool DisconnectAndWait(IConnection cnn) {
            var disconnected = false;
            Action dh = () => { disconnected = true; };
            cnn.Disconnected += dh;
            cnn.Disconnect();
            cnn.Disconnected -= dh;
            return disconnected;
        }

        static void Main() {
            var options = new[] { '1', '2', '3' };
            bool good;
            int selection;
            do {
                Console.WriteLine("Enter mode: 1 for library, 2 for manual, 3 for advanced manual, 4 to exit");
                var l = Console.ReadKey();
                selection = Array.IndexOf(options, l.KeyChar);
                good = selection >= 0;
                Console.WriteLine();
            } while (!good);


            switch (selection) {
                case 0:
                    Console.WriteLine("Lib Mode");
                    Logger = new ConsoleLogger(Level.Trace, "App");
                    var x = new Connection(Logger);
                    x.Connected += () => Console.WriteLine("OnConnected");
                    x.Disconnected += () => Console.WriteLine("OnDisconnected");
                    Console.WriteLine(ConnectAndWait(x, 10) ? "Connected yeah" : "Failed to connect");
                    Console.WriteLine(DisconnectAndWait(x) ? "Disconnected yeah" : "Failed to disconnect");
                    break;
                case 1:
                    Console.WriteLine("Manual Mode");
                    var api = new EikonDesktopDataAPI();
                    api.OnStatusChanged += status => Console.WriteLine("Status -> {0}", status.ToString());
                    api.Initialize();
                    Console.WriteLine("Wait for something or press any key to exit");

                    break;

                case 2:
                    Console.WriteLine("Advanced manual Mode - NOT IMPLEMENTED YET");
                    //bool connected;
                    //api = new EikonDesktopDataAPI();
                    //api.OnStatusChanged += status => {
                    //    Console.WriteLine("Status -> {0}", status.ToString());
                    //    if (status == EEikonStatus.Connected) {
                    //        connected = true;
                    //    }
                    //};


                    //api.Initialize();
                    //Console.WriteLine("Wait for something or press any key to exit");
                    break;
            } 
            Console.ReadKey();
        }
    }
}
