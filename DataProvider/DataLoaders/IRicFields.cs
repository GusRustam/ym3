using DataProvider.DataLoaders.Status;

namespace DataProvider.DataLoaders {
    public interface IRicFields {
        string[] Fields { get; }
        IItemStatus Status { get;  }
    }
}