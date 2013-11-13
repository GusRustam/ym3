namespace DataProvider.Loaders.Metadata {
    public interface IMetaRequestSetup {
        string Fields { get; }
        string DisplayMode { get;  }
        string RequestMode { get; }
    }
}