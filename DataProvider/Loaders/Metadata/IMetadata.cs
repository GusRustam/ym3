using System;

namespace DataProvider.Loaders.Metadata {
    /// <summary>
    /// Fluent API to Dex2
    /// </summary>
    public interface IMetadata {
        string[] Rics { get;  }
        IMetadata WithRics(params string[] rics);
        IMetadataReciever<TObject> Reciever<TObject>() where TObject : IMetadataFields, new();
        IMetadataReciever Reciever(Type type);
        IMetadataRequest Request();
    }
}