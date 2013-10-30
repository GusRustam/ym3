using System;

namespace Toolbox.Async {
    public interface ITimeoutCall {
        ITimeoutCall WithCallback(Action callback);
        ITimeoutCall WithTimeout(TimeSpan? timeout);
        void Request();
    }
}
