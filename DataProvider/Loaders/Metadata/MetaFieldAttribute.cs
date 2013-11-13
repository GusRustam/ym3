using System;

namespace DataProvider.Loaders.Metadata {
    public class MetaFieldAttribute : Attribute {
        private readonly string _name;

        public MetaFieldAttribute(string name) {
            _name = name;
        }

        public string Name {
            get { return _name; }
        }
    }
}