using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using EikonDesktopDataAPILib;
using LoggingFacility;
using LoggingFacility.LoggingSupport;

namespace Connect {
    public interface IConnection  {
        event Action Connected;
        event Action ConnectionTimeout;
        event Action Disconnected;
        event Action LocalMode;
        event Action Offline;

        EikonDesktopDataAPI Sdk { get; }
        EEikonStatus Status { get; }
        
        void Disconnect();
        void Connect(TimeSpan span);
    }

    public class Connection : IConnection, IDisposable, ISupportsLogging {
        public event Action Connected;
        public event Action ConnectionTimeout;
        public event Action Disconnected;
        public event Action LocalMode;
        public event Action Offline;

        private bool _disposed;
        public void Dispose() {
            this.Info("Dispose()");
            if (_disposed)
                return;

            Dispose(true);
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        protected virtual void Dispose(bool disposing) {
            this.Info("Dispose(bool)");
            if (!disposing)
                return;
            try {
                ClearSdk();
            } catch (Exception ex) {
                this.Warn("Failed to delete COM object", ex);
            }
        }

        private event Action InternalConnected;

        public EikonDesktopDataAPI Sdk { get; private set; }
        public EEikonStatus Status { get; private set; }

        public Connection(ILogger logger) {
            Status = EEikonStatus.Offline;
            Logger = logger;
        }

        private void OnStatusChanged(EEikonStatus estatus) {
            Status = estatus;
            switch (estatus) {
                case EEikonStatus.Connected:
                    if (InternalConnected != null) InternalConnected();    // first notifying internal users, 
                    if (Connected != null) Connected();                     // then doing external calls
                    break;
                case EEikonStatus.Disconnected:
                    if (Disconnected != null)
                        Disconnected();
                    break;
                case EEikonStatus.LocalMode:
                    if (LocalMode != null)
                        LocalMode();
                    break;
                case EEikonStatus.Offline:
                    if (Offline != null)
                        Offline();
                    break;
            }
        }

        private enum ConnectionState {
            Started,
            Succeeded,
            Failed
        }

        public void Disconnect() {
            ClearSdk();
            OnStatusChanged(EEikonStatus.Disconnected);
        }

        public void Connect(TimeSpan span) {
            //if (Sdk != null) ClearSdk();
            //Sdk = new EikonDesktopDataAPI();
            //Sdk.OnStatusChanged += OnStatusChanged;
            if (Sdk == null) {
                Sdk = new EikonDesktopDataAPI();
                Sdk.OnStatusChanged += OnStatusChanged;
            }

            // ReSharper disable UnusedVariable
            var status = Sdk.Status;
            // ReSharper restore UnusedVariable

            if (Sdk.Status == EEikonStatus.Connected) {
                OnStatusChanged(EEikonStatus.Connected);
            } else {
                var res = Sdk.Initialize();

                if (res != EEikonDataAPIInitializeResult.Succeed) {
                    OnStatusChanged(EEikonStatus.Offline);
                } else {
                    var cancelSrc = new CancellationTokenSource();
                    Task.Factory.StartNew(() => {
                        ConnectionState[] state = { ConnectionState.Started }; // initial state

                        // creating action to handle connection
                        Action rememberConnected = () => {
                            this.Trace("On Connected()");
                            lock (state) {
                                if (state[0] != ConnectionState.Started) return;
                                state[0] = ConnectionState.Succeeded;
                                this.Trace("Interrupting waiter thread");
                                cancelSrc.Cancel();
                            }
                        };

                        InternalConnected += rememberConnected; // attach catcher
                        this.Trace("Starting waiting");
                        cancelSrc.Token.WaitHandle.WaitOne(span);
                        this.Trace("Finished waiting");
                        InternalConnected -= rememberConnected; // detach catcher

                        // what's happend while the thread slep?
                        lock (state) {
                            // state has not changed from started => it's still connecting
                            if (state[0] != ConnectionState.Started) return;
                            state[0] = ConnectionState.Failed;
                            if (ConnectionTimeout != null)
                                ConnectionTimeout();
                        }
                    }, cancelSrc.Token);
                }
            }
        }

        private void ClearSdk() {
            this.Info("ClearSDK()");
            if (Sdk == null) return;
            Sdk.OnStatusChanged -= OnStatusChanged;
            Marshal.FinalReleaseComObject(Sdk);
            Sdk = null;
        }

        public ILogger Logger { get; set; }
    }
}