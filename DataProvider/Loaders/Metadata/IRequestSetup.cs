using System;

namespace DataProvider.Loaders.Metadata {
    public interface IRequestSetup<T> where T : IMetadataItem {
        MetaFieldInfo[] FieldInfo { get; set; }
        string[] Fields { get; }
        string[] Rics { get; set; }
        string DisplayMode { get; set; }
        string RequestMode { get; set; }
        Action<IMetadataContainer<T>> Callback { get; set; }
    }
}