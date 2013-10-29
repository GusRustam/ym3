using System.Collections.Generic;
using DataProvider.DataLoaders.Status;

namespace DataProvider.DataLoaders {
    public interface ISnapshot {
        IEnumerable<ISnapshotItem> Data { get; }
        ISourceStatus SourceStatus { get; }
        IListStatus ListStatus { get; }
    }
}