using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataProvider.DataLoaders.Status;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using ThomsonReuters.Interop.RTX;

namespace DataProvider.DataLoaders {
    internal abstract class AdfinTimeoutRequest : ISupportsLogging, ITimeout {
        // State machine
        protected enum State {
            Init,
            Timeout,
            Succeded,
            Invalid
        }

        // Callbacks
        private Action _timeout;
        
        // Internal state
        protected State InternalState;

        // Outer data
        protected readonly string[] Rics;
        protected readonly AdxRtList AdxRtList;
        protected readonly TimeSpan? WaitTime;
        protected readonly string Feed;

        // Threading data
        protected readonly object LockObj = new object();
        protected CancellationTokenSource CancelSrc;

        // Abstract operations
        protected abstract void RegisterHandlers();
        protected abstract void ReportInvalid();
        protected abstract void ReportOk();
        protected abstract IRunMode AdfinMode { get; }


        /// <summary>
        /// Use specific timeout callback
        /// </summary>
        /// <param name="callback">Callback</param>
        public ITimeout WithTimeout(Action callback) {
            _timeout = callback;
            return this;
        }

        protected AdfinTimeoutRequest(ILogger logger, AdxRtList adxRtList, TimeSpan? waitTime, string[] rics, string feed) {
            AdxRtList = adxRtList;
            WaitTime = waitTime;
            Rics = rics;
            Logger = logger;
            Feed = feed;
        }

        /// <summary>
        /// Request for fields
        /// </summary>
        public void Request() {
            if (!Rics.Any())
                throw new ArgumentException("rics");

            AdxRtList.Source = Feed;
            AdxRtList.Mode = "";
            AdxRtList.ErrorMode = AdxErrorMode.EXCEPTION;
            AdxRtList.DebugLevel = RT_DebugLevel.RT_DEBUG_IMMEDIATE;

            RegisterHandlers();

            InternalState = State.Init;
            CancelSrc = new CancellationTokenSource();

            // starting a thread which will receive data
            Task.Factory.StartNew(() => RequestFields(AdfinMode), CancelSrc.Token);
        }


        private void RequestFields(IRunMode mode) {
            this.Trace("Started thread");

            // request data
            AdxRtList.StartUpdates(mode.ToAdxMode());

            // and waits for cancellation or receipt of all data
            CancelSrc.Token.WaitHandle.WaitOne(
                WaitTime.HasValue ?
                    WaitTime.Value :
                    TimeSpan.FromMilliseconds(-1));

            // waiting finished
            this.Trace("Waiting finished");

            // closing links
            AdxRtList.CloseAllLinks();
            lock (LockObj) {
                this.Trace(string.Format("State {0}", InternalState));
                TryChangeState(State.Timeout);
                switch (InternalState) {
                    case State.Timeout:
                        if (_timeout != null)
                            _timeout();
                        break;
                    case State.Invalid:
                        ReportInvalid();
                        break;
                    default:
                        ReportOk();
                        break;
                }
            }
        }

        /// <summary>
        /// Changes state only if state hasn't been already changed
        /// </summary>
        /// <param name="newState">new state</param>
        protected void TryChangeState(State newState) {
            if (InternalState != State.Init) return;
            InternalState = newState;
            if (!CancelSrc.IsCancellationRequested)
                CancelSrc.Cancel();
        }

        public ILogger Logger { get; private set; }
    }
}