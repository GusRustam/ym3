using System;
using System.Threading;
using System.Threading.Tasks;
using LoggingFacility;
using LoggingFacility.LoggingSupport;

namespace Toolbox.Async {
    public abstract class TimeoutCall : ITimeoutCall, ISupportsLogging {
        /// <summary>
        /// Use it to synchronize event handing
        /// </summary>
        protected readonly object LockObj = new object();

        /// <summary> 
        /// State of call 
        /// </summary>
        protected enum State {
            Init,
            Timeout,
            Succeded,
            Invalid,
            Cancelled
        }

        /// <summary> Use it to register an exception which failed the call </summary>
        protected Exception Report {
            set { _report = value; }
        }

        // Protected virtual items - use it to implement specific behavior
        /// <summary>  
        /// Does all preparation
        /// </summary>
        protected abstract void Prepare();

        /// <summary>  
        /// Starts async calls  
        /// </summary>
        protected abstract void Perform();

        /// <summary> 
        /// Specific actions to inform user that everything is fine 
        /// </summary>
        protected abstract void Finish();
        protected abstract void HandleTimout();
        protected abstract void HandleError(Exception ex);
        protected abstract void HandleCancel();


        // Object state
        private CancellationTokenSource _cancelSrc;
        private TimeSpan? _timeout;
        private Exception _report;
        private State _internalState;

        protected TimeoutCall(ILogger logger) {
            Logger = logger;
        }

        protected State InternalState {
            get { return _internalState; }
        }

        #region ITimeoutCall implementation
        public ITimeoutCall WithTimeout(TimeSpan? timeout) {
            _timeout = timeout;
            return this;
        }

        //public void Request() {
        //    try {
        //        Prepare();
        //    } catch (Exception e) {
        //        Report = e;
        //        TryChangeState(State.Invalid);
        //        Return();
        //        return;
        //    }

        //    _cancelSrc = new CancellationTokenSource();
        //    Task.Factory.StartNew(() => {
        //        try {
        //            Perform();
        //        } catch (Exception e) {
        //            Report = e;
        //            TryChangeState(State.Invalid);
        //            Return();
        //            return;
        //        }

        //        // and waits for cancellation or receipt of all data
        //        _cancelSrc.Token.WaitHandle.WaitOne(
        //            _timeout.HasValue
        //                ? _timeout.Value
        //                : TimeSpan.FromMilliseconds(-1));

        //        Return();

        //    }, _cancelSrc.Token);

        //}

        //private void Return() {
        //    lock (LockObj) {
        //        TryChangeState(State.Timeout);
        //        switch (_internalState) {
        //            case State.Timeout:
        //                if (_callback != null)
        //                    _callback();
        //                break;

        //            case State.Invalid:
        //                if (_error != null)
        //                    _error(_report);
        //                break;

        //            case State.Cancelled:
        //                if (_cancel != null)
        //                    _cancel();
        //                break;

        //            case State.Succeded:
        //                Success();
        //                break;
        //        }
        //    }
        //}

        public void Request() {
            this.Trace("Request()");
            Prepare();
            _cancelSrc = new CancellationTokenSource();
            Task.Factory.StartNew(
                () => {
                    this.Trace("In task, to perform");
                    Perform();
                    this.Trace("Performed, to wait");
                    // and waits for cancellation or receipt of all data
                        _cancelSrc.Token.WaitHandle.WaitOne(
                        _timeout.HasValue
                            ? _timeout.Value
                            : TimeSpan.FromMilliseconds(-1));
                
                    lock (LockObj) {
                        this.Trace("Waiting finished");
                        TryChangeState(State.Timeout);
                        switch (_internalState) {
                            case State.Timeout:
                                HandleTimout();
                                break;

                            case State.Invalid:
                                HandleError(_report);
                                break;

                            case State.Cancelled:
                                HandleCancel();
                                break;
                        }
                        Finish();
                    }
                }, _cancelSrc.Token)
            .ContinueWith(
                task => {
                    // there's really no way I could deliver to user unless he has separate callback
                    // OnError. But even this callback might return error
                    if (task.Exception == null) return;
                    this.Trace(string.Format("Caught exception in thread\n{0}", task.Exception));
                    _report = task.Exception;
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void Cancel() {
            TryChangeState(State.Cancelled);
        }

        #endregion

        /// <summary>
        /// Changes state only if state hasn't been already changed
        /// </summary>
        /// <param name="newState">new state</param>
        protected void TryChangeState(State newState) {
            lock (LockObj) {
                this.Trace(string.Format("Trying state {0} -> {1}", _internalState, newState));
                if (_internalState != State.Init)
                    return;
                _internalState = newState;
                this.Trace(string.Format("State now {0}", _internalState));
                this.Trace("To request cancellation");
                if (!_cancelSrc.IsCancellationRequested) {
                    _cancelSrc.Cancel();
                    this.Trace("Cancellation requested");
                } else {
                    this.Trace("Cancellation already requested");
                }
            }
        }

        public ILogger Logger { get; private set; }
    }
}