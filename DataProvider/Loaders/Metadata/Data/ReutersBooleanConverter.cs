namespace DataProvider.Loaders.Metadata.Data {
    public class ReutersBooleanConverter : IMetadataConverter {
        public object Decode(string arg) {
            return arg.Trim() == "Y";
        }
    }
}