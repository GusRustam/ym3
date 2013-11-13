using System;

namespace DataProvider.Loaders.Metadata {
    public class MetaColumnAttribute : Attribute {
        private readonly int _index;

        public MetaColumnAttribute(int index) {
            _index = index;
        }

        public int Index {
            get { return _index; }
        }
    }
}