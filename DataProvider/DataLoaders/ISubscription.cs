using System;
using DataProvider.DataLoaders.Status;

namespace DataProvider.DataLoaders {
    public interface ISubscription {
        ISubscription OnTime(Action action);
        ISubscription OnDataUpdated(Action<string, object, IItemStatus> acton);
        
        void Start(IRunMode mode);
        void Stop();
        void Close();
    }
}