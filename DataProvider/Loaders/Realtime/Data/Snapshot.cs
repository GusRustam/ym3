using System.Collections.Generic;
using System.Collections.ObjectModel;
using DataProvider.Loaders.Status;

namespace DataProvider.Loaders.Realtime.Data {
    public class Snapshot : ISnapshot {
        //private readonly ISourceStatus _sourceStatus;
        private readonly List<ISnapshotItem> _data;

        public IEnumerable<ISnapshotItem> Data {
            get { return new ReadOnlyCollection<ISnapshotItem>(_data); }
        }

        public ISourceStatus SourceStatus {
            get; private set;
        }

        public IListStatus ListStatus { get; private set; }

        public Snapshot() {
            _data = new List<ISnapshotItem>();
        }

        public Snapshot(ISourceStatus sourceStatus, IListStatus listStatus, List<ISnapshotItem> data) {
            SourceStatus = sourceStatus;
            ListStatus = listStatus;
            _data = data;
        }

        public Snapshot(ISourceStatus sourceStatus, IListStatus listStatus) {
            SourceStatus = sourceStatus;
            ListStatus = listStatus;
            _data = null;
        }

        public void Add(ISnapshotItem item) {
            _data.Add(item);
        }
    }
}