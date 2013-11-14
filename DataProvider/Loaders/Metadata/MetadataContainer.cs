using System.Collections.Generic;
using DataProvider.Loaders.Status;

namespace DataProvider.Loaders.Metadata {
    public class MetadataContainer<T> : IMetadataContainer<T> where T : IMetadataItem {
        private List<MetaFieldInfo> _fieldsInfo;

        public MetadataContainer(List<MetaFieldInfo> fieldsInfo) {
            _fieldsInfo = fieldsInfo;
        }

        public IDataStatus Status { get; set; }
        public IEnumerable<T> Rows { get; set; }
        

        public void ImportRow(object[] row) {
            throw new System.NotImplementedException();
        }
    }
}