using System;
using DataProvider.Loaders.Realtime.Data;
using DataProvider.Loaders.Status;

namespace DataProvider.Loaders.Realtime {
    public interface ISubscription {
        ISubscription WithCallback(Action<ISnapshot> acton);
        ISubscription WithStatus(Action<string, ISourceStatus, IListStatus> action);
        
        /// <summary>
        /// Subscribes on updates using selected mode.
        /// </summary>
        /// <param name="mode"></param>
        /// <remarks>
        ///     If you need Image mode, use class <see cref="ISnapshotTicker"/>
        /// </remarks>
        void Start(IRunMode mode);
        void Stop();
        void Close();
    }
}