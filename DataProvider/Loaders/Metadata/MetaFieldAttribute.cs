using System;

namespace DataProvider.Loaders.Metadata {
    public class MetaFieldAttribute : Attribute {
        private readonly int _order;
        private readonly string _name;
        private readonly Type _converter;

        public MetaFieldAttribute(int order, string name = null, Type converter = null) {
            _order = order;
            _name = name;
            _converter = converter;
        }

        public string Name {
            get { return _name; }
        }

        public int Order {
            get { return _order; }
        }

        public Type Converter {
            get { return _converter; }
        }
    }
}