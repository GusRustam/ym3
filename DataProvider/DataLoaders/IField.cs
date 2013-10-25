using DataProvider.DataLoaders.Status;

namespace DataProvider.DataLoaders {
    public interface IField {
        string Name { get; }
        string Value { get; }
        IFieldStatus Status { get; }
    }
}