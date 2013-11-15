using DataProvider.Loaders.Metadata.Data;

namespace DataProvider.Loaders.Metadata {
    public class RequestSetupBase {
        public MetaFieldInfo[] FieldInfo { get; set; }
        public string DisplayMode { get; set; }
        public string RequestMode { get; set; }
    }
}