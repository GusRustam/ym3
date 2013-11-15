namespace DataProvider.Loaders.Metadata.Data {
    public interface IMetadataImporter<T> where T : IMetadataItem, new() {
        T Import(object[] currentRow);
    }
}