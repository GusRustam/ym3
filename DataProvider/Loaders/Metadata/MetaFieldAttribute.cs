using System;

namespace DataProvider.Loaders.Metadata {
    public class MetaFieldAttribute : Attribute {
        private readonly int _order;
        private readonly string _name;

        public MetaFieldAttribute(int order, string name = null) {
            _order = order;
            _name = name;
        }

        public string Name {
            get { return _name; }
        }

        public int Order {
            get { return _order; }
        }
    }
}