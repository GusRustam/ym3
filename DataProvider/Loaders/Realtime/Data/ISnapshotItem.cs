using System.Collections.Generic;
using DataProvider.Loaders.Status;

namespace DataProvider.Loaders.Realtime.Data {
    public interface ISnapshotItem {
        string Ric { get; }
        IItemStatus Status { get; }
        IEnumerable<IField> Fields { get; }
    }
}