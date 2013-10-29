using DataProvider.Loaders.Status;

namespace DataProvider.Loaders.Realtime.Data {
    public interface IRicFields {
        string[] Fields { get; }
        IItemStatus Status { get;  }
    }
}