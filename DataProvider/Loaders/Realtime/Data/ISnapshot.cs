using System.Collections.Generic;
using DataProvider.Loaders.Status;

namespace DataProvider.Loaders.Realtime.Data {
    public interface ISnapshot {
        IEnumerable<ISnapshotItem> Data { get; }
        ISourceStatus SourceStatus { get; }
        IListStatus ListStatus { get; }
    }
}