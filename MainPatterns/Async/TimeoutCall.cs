using System;
using System.Threading;
using System.Threading.Tasks;

namespace Toolbox.Async {
    public abstract class TimeoutCall : ITimeoutCall {
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
        /// Specific actions to inform user that everything's fine 
        /// </summary>
        protected abstract void Finish();

        // Object state
        private CancellationTokenSource _cancelSrc;
        private TimeSpan? _timeout;
        private Exception _report;
        private State _internalState;

        //// Callbacks
        //private Action<Exception> _error;
        //private Action _callback;
        //private Action _cancel;

        #region ITimeoutCall implementation

        //public ITimeoutCall WithCancelCallback(Action callback) {
        //    _cancel = callback;
        //    return this;
        //}

        //public ITimeoutCall WithTimeoutCallback(Action callback) {
        //    _callback = callback;
        //    return this;
        //}

        //public ITimeoutCall WithErrorCallback(Action<Exception> callback) {
        //    _error = callback;
        //    return this;
        //}

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
            Prepare();
            _cancelSrc = new CancellationTokenSource();
            Task.Factory.StartNew(() => {
                Perform();
                // and waits for cancellation or receipt of all data
                _cancelSrc.Token.WaitHandle.WaitOne(
                    _timeout.HasValue
                        ? _timeout.Value
                        : TimeSpan.FromMilliseconds(-1));
                lock (LockObj) {
                    TryChangeState(State.Timeout);
                    //HandleState(_internalState);
                    switch (_internalState) {
                        case State.Timeout:
                            //if (_callback != null) _callback();
                            HandleTimout();
                            break;

                        case State.Invalid:
                            //if (_error != null)
                            //    _error(_report);
                            HandleError(_report);
                            break;

                        case State.Cancelled:
                            HandleCancel();
                            //if (_cancel != null)
                            //    _cancel();
                            break;

                        //case State.Succeded:
                        //    Success();
                        //    break;
                    }
                    Finish();
                }
            }, _cancelSrc.Token);
        }

        protected abstract void HandleTimout();
        protected abstract void HandleError(Exception ex);
        protected abstract void HandleCancel();

        public void Cancel() {
            TryChangeState(State.Cancelled);
        }

        #endregion

        /// <summary>
        /// Changes state only if state hasn't been already changed
        /// </summary>
        /// <param name="newState">new state</param>
        protected void TryChangeState(State newState) {
            if (_internalState != State.Init)
                return;
            _internalState = newState;
            if (!_cancelSrc.IsCancellationRequested)
                _cancelSrc.Cancel();
        }

    }
}