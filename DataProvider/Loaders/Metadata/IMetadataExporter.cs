namespace DataProvider.Loaders.Metadata {
    public interface IMetadataExporter<T> where T : IMetadataItem {
        RequestSetupBase GetMetaParams();
    }
}