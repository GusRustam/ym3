namespace DataProvider.Loaders.Metadata.Data {
    public interface IMetadataExporter<T> where T : IMetadataItem {
        RequestSetupBase GetMetaParams();
    }
}