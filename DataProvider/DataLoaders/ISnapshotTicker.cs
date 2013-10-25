using System;

namespace DataProvider.DataLoaders {
    public interface ISnapshotTicker : ITimeout {
        ISnapshotTicker WithCallback(Action<ISnapshot> onImage);
    }
}