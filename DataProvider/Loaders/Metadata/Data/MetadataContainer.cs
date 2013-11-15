using System.Collections.Generic;
using DataProvider.Loaders.Status;

namespace DataProvider.Loaders.Metadata.Data {
    public class MetadataContainer<T> : IMetadataContainer<T> where T : IMetadataItem, new() {
        public MetadataContainer() {
            Rows = new List<T>();
        }

        public IDataStatus Status { get; set; }

        public IList<T> Rows {
            get; private set;
        }
    }
}