namespace DataProvider.Loaders.Metadata {
    public class ExpressionMetadataImporter<T> : IMetadataImporter<T> where T : IMetadataItem, new() {
        public T Import(object[] currentRow) {
            throw new System.NotImplementedException();
        }
    }
}