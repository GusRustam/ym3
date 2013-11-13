using System;

namespace DataProvider.Loaders.Metadata {
    public interface IMetaRequestSetup<T> where T : IMetadataItem {
        string[] Rics { get; set; }
        string Fields { get; set; }
        string DisplayMode { get; set; }
        string RequestMode { get; set; }
        Action<IMetadataContainer<T>> Callback { get; set; }
    }
}