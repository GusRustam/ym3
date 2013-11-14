using System.Collections.Generic;
using DataProvider.Loaders.Status;

namespace DataProvider.Loaders.Metadata {
    /// <summary>
    /// This class represents a container of metadata objects, retrieved from DB
    /// </summary>
    /// <remarks>
    /// The class which implements this interface has to contain logic to filter out 
    /// invalid entries. For example, bond must have BondStructure, and in case 
    /// this field is empty, it should not be visible.
    /// </remarks>
    public interface IMetadataContainer<T> where T : IMetadataItem {
        IDataStatus Status { get; set; }
        IEnumerable<T> Rows { get; set; }
        void ImportRow(object[] row);
    }
}