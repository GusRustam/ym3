namespace DataProvider.Loaders.Metadata.Data {
    /// <summary> This class represents an object, into which data from DB is to be loaded </summary>
    /// <remarks>
    /// The class, which will derive from this interface, will have to implement ways to filter and sort, for example
    /// <b>IMPORTANT: in each subclass and item of order 0 must be ric or isin</b>
    /// </remarks>
    public interface IMetadataItem {
    }
}