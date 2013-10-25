using System.Collections.Generic;
using DataProvider.DataLoaders.Status;

namespace DataProvider.DataLoaders {
    public interface ISnapshot {
        IEnumerable<ISnapshotItem> Data { get; }
        ISourceStatus Status { get; }
    }
}