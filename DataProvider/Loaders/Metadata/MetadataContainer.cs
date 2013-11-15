using System.Collections.Generic;
using DataProvider.Loaders.Status;

namespace DataProvider.Loaders.Metadata {
    public class MetadataContainer<T> : IMetadataContainer<T> where T : IMetadataItem, new() {
        //private readonly List<MetaFieldInfo> _fieldsInfo;

        public MetadataContainer(/*List<MetaFieldInfo> fieldsInfo*/) {
            //_fieldsInfo = fieldsInfo;
            Rows = new List<T>();
        }

        public IDataStatus Status { get; set; }

        public IList<T> Rows {
            get; private set;
        }
    }
}