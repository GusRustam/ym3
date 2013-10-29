using System;
using DataProvider.DataLoaders.Status;

namespace DataProvider.DataLoaders {
    public interface ISubscription {
        ISubscription Callback(Action<ISnapshot> acton);
        ISubscription OnStatus(Action<string, ISourceStatus, IListStatus> action);
        
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