using System;
using Toolbox.Async;

namespace DataProvider.Loaders.Chain {
    public class MultiChainRequest : IChainRequest {
        public ITimeoutCall WithCancelCallback(Action callback) {
            throw new NotImplementedException();
        }

        public ITimeoutCall WithTimeoutCallback(Action callback) {
            throw new NotImplementedException();
        }

        public ITimeoutCall WithErrorCallback(Action<Exception> callback) {
            throw new NotImplementedException();
        }

        public ITimeoutCall WithTimeout(TimeSpan? timeout) {
            throw new NotImplementedException();
        }

        public void Request() {
            throw new NotImplementedException();
        }

        public void Cancel() {
            throw new NotImplementedException();
        }
    }
}