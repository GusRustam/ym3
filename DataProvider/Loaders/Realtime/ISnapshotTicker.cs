using System;
using DataProvider.Loaders.Realtime.Data;

namespace DataProvider.Loaders.Realtime {
    public interface ISnapshotTicker : ITimeout {
        ISnapshotTicker WithCallback(Action<ISnapshot> onImage);
    }
}