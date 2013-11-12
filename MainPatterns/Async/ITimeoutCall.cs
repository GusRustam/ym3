using System;

namespace Toolbox.Async {
    /// <summary> General interface to async calls </summary>
    public interface ITimeoutCall {
        ///// <summary>
        ///// Set function to call when cancelled
        ///// </summary>
        ///// <param name="callback">Callback function</param>
        //ITimeoutCall WithCancelCallback(Action callback);

        ///// <summary>
        ///// Set function to call when time's out
        ///// </summary>
        ///// <param name="callback">Callback function</param>
        //ITimeoutCall WithTimeoutCallback(Action callback);

        ///// <summary>
        ///// Set function to call when any error occured
        ///// </summary>
        ///// <param name="callback">Callback function</param>
        //ITimeoutCall WithErrorCallback(Action<Exception> callback);

        /// <summary>
        /// Set time span until timeout
        /// </summary>
        /// <param name="timeout">TimeSpan to wait for specified time, null to wait infinitely</param>
        ITimeoutCall WithTimeout(TimeSpan? timeout);

        /// <summary> Request data </summary>
        void Request();

        /// <summary> Stop waiting for data </summary>
        void Cancel();
    }
}
