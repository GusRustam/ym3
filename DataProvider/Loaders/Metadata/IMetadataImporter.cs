namespace DataProvider.Loaders.Metadata {
    public interface IMetadataImporter<T> where T : IMetadataItem, new() {
        T Import(object[] currentRow);
    }
}