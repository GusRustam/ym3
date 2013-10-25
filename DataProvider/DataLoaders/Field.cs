using DataProvider.DataLoaders.Status;

namespace DataProvider.DataLoaders {
    public class Field : IField {
        private readonly string _name;
        private readonly string _value;
        private readonly IFieldStatus _status;

        public Field(string name, string value, IFieldStatus status) {
            _name = name;
            _value = value;
            _status = status;
        }

        public string Name {
            get { return _name; }
        }

        public string Value {
            get { return _value; }
        }

        public IFieldStatus Status {
            get { return _status; }
        }
    }
}