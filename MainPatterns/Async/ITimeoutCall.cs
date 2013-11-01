using System;

namespace Toolbox.Async {
    public interface ITimeoutCall {
        ITimeoutCall WithCancelCallback(Action callback);
        ITimeoutCall WithTimeoutCallback(Action callback);
        ITimeoutCall WithErrorCallback(Action<Exception> callback);
        ITimeoutCall WithTimeout(TimeSpan? timeout);
        void Request();
        void Cancel();
    }
}
