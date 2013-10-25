using System.Collections.Generic;
using System.Collections.ObjectModel;
using DataProvider.DataLoaders.Status;

namespace DataProvider.DataLoaders {
    public class Snapshot : ISnapshot {
        private readonly ISourceStatus _status;
        private readonly List<ISnapshotItem> _data;

        public IEnumerable<ISnapshotItem> Data {
            get { return new ReadOnlyCollection<ISnapshotItem>(_data); }
        }

        public ISourceStatus Status {
            get { return _status; }
        }

        public Snapshot() {
            _data = new List<ISnapshotItem>();
        }

        public Snapshot(ISourceStatus status, List<ISnapshotItem> data) {
            _status = status;
            _data = data;
        }

        public Snapshot(ISourceStatus status) {
            _status = status;
            _data = null;
        }

        public void Add(ISnapshotItem item) {
            _data.Add(item);
        }
    }
}