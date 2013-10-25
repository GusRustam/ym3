using System;
using DataProvider.DataLoaders.Status;

namespace DataProvider.DataLoaders {
    public interface ISnapshotTicker : ITimeout {
        ISnapshotTicker WithCallback(Action<IDataStatus> onImage);
    }
}