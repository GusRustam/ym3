using DataProvider.Loaders.Status;

namespace DataProvider.Loaders.Realtime.Data {
    public interface IField {
        string Name { get; }
        string Value { get; }
        IFieldStatus Status { get; }
    }
}