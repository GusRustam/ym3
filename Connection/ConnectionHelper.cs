using System;
using System.Threading;
using System.Threading.Tasks;
using LoggingFacility.LoggingSupport;

namespace Connect {
    public static class ConnectionHelper {
        public static bool ConnectAndWait(this IConnection cnn, int seconds) {
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
                var lg = cnn as ISupportsLogging;
                if (lg != null) lg.Info("OperationCanceledException is okay");
                try {
                    var bond = cnn.Sdk.CreateAdxBondModule();
                    connected = bond != null;
                } catch (Exception) {
                    if (lg != null) lg.Info("failed to create bond is not okay");
                    connected = false;
                }
            }

            // when it's finished (either by cancel or by timeout), remove Connected handler
            cnn.Connected -= cn;

            // if cancellation was requested, this means that Connected event had 
            // happened, otherwise no connection was established
            return connected;
        }

        public static bool DisconnectAndWait(this IConnection cnn) {
            var disconnected = false;
            Action dh = () => { disconnected = true; };
            cnn.Disconnected += dh;
            cnn.Disconnect();
            cnn.Disconnected -= dh;
            return disconnected;
        }
    }
}
