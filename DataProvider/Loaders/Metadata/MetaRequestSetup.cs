using System;

namespace DataProvider.Loaders.Metadata {
    public class MetaRequestSetup<T> : IMetaRequestSetup<T> where T : IMetadataItem {
        public string[] Rics { get; set; }
        public string Fields { get; set; }
        public string DisplayMode { get; set; }
        public string RequestMode { get; set; }
        public Action<IMetadataContainer<T>> Callback { get; set; }
    }
}