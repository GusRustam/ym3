namespace DataProvider.Loaders.Metadata {
    /// <summary>
    ///  This interface is particularly useful for mocking
    /// </summary>
    public interface IMetaObjectFactory<T> where T : IMetadataItem, new() {
        MetadataRequest<T>.MetadataRequestAlgo CreateAlgo(IMetaRequestSetup<T> setup); // todo ugly

        IMetadataRequest<T> CreateRequest(IMetaRequestSetup<T> setup);
        IMetaRequestSetup<T> CreateSetup();
        IMetadataContainer<T> CreateContainer();
    }
}