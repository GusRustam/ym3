using System.Collections.Generic;
using System.Collections.ObjectModel;
using DataProvider.Loaders.Status;

namespace DataProvider.Loaders.Realtime.Data {
    public class SnapshotItem : ISnapshotItem {

        private readonly string _ric;
        private readonly IItemStatus _status;
        private readonly List<IField> _fields;

        public SnapshotItem(string ric, IItemStatus status) {
            _ric = ric;
            _status = status;
            _fields = new List<IField>();
        }

        public string Ric {
            get { return _ric; }
        }

        public IItemStatus Status {
            get { return _status; }
        }

        public IEnumerable<IField> Fields {
            get { return new ReadOnlyCollection<IField>(_fields); }
        }

        public void AddField(string fieldName, string fieldValue, IFieldStatus fieldStatus) {
            _fields.Add(new Field(fieldName, fieldValue, fieldStatus));
        }
    }
}