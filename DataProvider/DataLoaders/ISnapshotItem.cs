using System.Collections.Generic;
using DataProvider.DataLoaders.Status;

namespace DataProvider.DataLoaders {
    public interface ISnapshotItem {
        string Ric { get; }
        IItemStatus Status { get; }
        IEnumerable<IField> Fields { get; }
    }
}