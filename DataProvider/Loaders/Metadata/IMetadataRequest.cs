using Toolbox.Async;

namespace DataProvider.Loaders.Metadata {
    /// <summary> Stub to create type-specific timeout call </summary>
    public interface IMetadataRequest<T> : ITimeoutCall 
        where T : IMetadataItem {
    }
}