using System;
using System.Threading;
using System.Threading.Tasks;

namespace Toolbox.Async {
    public abstract class TimeoutCall : ITimeoutCall {
        protected readonly object LockObj = new object();
        protected CancellationTokenSource CancelSrc;

        private TimeSpan? _timeout;
        private Action _callback;

        // Internal state
        protected State InternalState;

        protected enum State {
            Init,
            Timeout,
            Succeded,
            Invalid
        }
        public ITimeoutCall WithCallback(Action callback) {
            _callback = callback;
            return this;
        }

        public ITimeoutCall WithTimeout(TimeSpan? timeout) {
            _timeout = timeout;
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
                            if (_callback != null)
                                _callback();
                            break;
                        case State.Invalid:
                            ReportInvalid();
                            break;
                        default:
                            Success();
                            break;
                    }
                }
            }, CancelSrc.Token);
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

        protected virtual void ReportInvalid() {
            if (_callback != null)
                _callback();
        }

        protected abstract void Perform();
        protected abstract void Prepare();
        protected abstract void Success();

    }
}