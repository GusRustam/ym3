using System;
using DataProvider.DataLoaders.Status;

namespace DataProvider.DataLoaders {
    public interface ISubscription {
        ISubscription Callback(Action<ISnapshot> acton);
        ISubscription OnStatus(Action<string, ISourceStatus, IListStatus> action);
        
        void Start(IRunMode mode);
        void Stop();
        void Close();
    }
}