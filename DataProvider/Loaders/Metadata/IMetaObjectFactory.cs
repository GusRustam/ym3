namespace DataProvider.Loaders.Metadata {
    /// <summary>
    ///  This interface is particularly useful for mocking
    /// </summary>
    public interface IMetaObjectFactory {
        IMetadataReciever<T> CreateReciever<T>() where T : IMetadataFields, new();
        IMetadataReciever CreateReciever();
        IMetadataRequest CreateRequest(IMetadata metadata);
        MetadataRequest.MetadataRequestAlgo CreateAlgo(IMetaRequestSetup setup);
    }
}