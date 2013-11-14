namespace DataProvider.Loaders.Metadata {
    /// <summary>
    ///  This interface is particularly useful for mocking
    /// </summary>
    public interface IMetaObjectFactory<T> where T : IMetadataItem, new() {
        MetadataRequest<T>.MetadataRequestAlgo CreateAlgo(IRequestSetup<T> setup); // todo ugly

        IMetadataRequest<T> CreateRequest(IRequestSetup<T> setup);
        IRequestSetup<T> CreateSetup();
        IMetadataContainer<T> CreateContainer(IRequestSetup<T> setup);
    }
}