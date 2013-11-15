namespace DataProvider.Loaders.Metadata {
    public class ReutersBooleanConverter : IMetadataConverter {
        public object Decode(string arg) {
            return arg.Trim() == "Y";
        }
    }
}