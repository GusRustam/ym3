using System;
using System.Threading;
using System.Threading.Tasks;

namespace Toolbox.Async {
    public abstract class TimeoutCall : ITimeoutCall {
        protected readonly object LockObj = new object();
        protected CancellationTokenSource CancelSrc;

        private TimeSpan? _timeout;
        private Exception _report;

        // Internal state
        protected State InternalState;

        // callbacks
        private Action<Exception> _error;
        private Action _callback;
        private Action _cancel;

        protected enum State {
            Init,
            Timeout,
            Succeded,
            Invalid,
            Cancelled
        }

        protected Exception Report {
            set { _report = value; }
        }

        public ITimeoutCall WithCancelCallback(Action callback) {
            _cancel = callback;
            return this;
        }

        public ITimeoutCall WithTimeoutCallback(Action callback) {
            _callback = callback;
            return this;
        }

        public ITimeoutCall WithErrorCallback(Action<Exception> callback) {
            _error = callback;
            return this;
        }

        public ITimeoutCall WithTimeout(TimeSpan? timeout) {
            _timeout = timeout;
            return this;
        }

        public void Request() {
            Prepare();
            CancelSrc = new CancellationTokenSource();
            Task.Factory.StartNew(() => {
                Perform();
                // and waits for cancellation or receipt of all data
                CancelSrc.Token.WaitHandle.WaitOne(
                    _timeout.HasValue ?
                        _timeout.Value :
                        TimeSpan.FromMilliseconds(-1));
                lock (LockObj) {
                    TryChangeState(State.Timeout);
                    switch (InternalState) {
                        case State.Timeout:
                            if (_callback != null) _callback();
                            break;

                        case State.Invalid:
                            if (_error != null) _error(_report);
                            break;

                        case State.Cancelled:
                            if (_cancel != null) _cancel();
                            break;

                        case State.Succeded:
                            Success();
                            break;
                    }
                }
            }, CancelSrc.Token);
        }

        public void Cancel() {
            TryChangeState(State.Cancelled);
        }

        /// <summary>
        /// Changes state only if state hasn't been already changed
        /// </summary>
        /// <param name="newState">new state</param>
        protected void TryChangeState(State newState) {
            if (InternalState != State.Init)
                return;
            InternalState = newState;
            if (!CancelSrc.IsCancellationRequested)
                CancelSrc.Cancel();
        }

        protected abstract void Perform();
        protected abstract void Prepare();
        protected abstract void Success();

    }
}