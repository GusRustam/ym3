using System;
using DataProvider.Loaders.Metadata.Data;

namespace DataProvider.Loaders.Metadata {
    /// <summary>
    /// Fluent API to Dex2
    /// </summary>
    public interface IMetadata<T> where T : IMetadataItem, new() {
        IMetadata<T> WithRics(params string[] rics);
        IMetadata<T> OnFinished(Action<IMetadataContainer<T>> action);
        IMetadataRequest<T> Request();
    }
}