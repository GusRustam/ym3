using System;

namespace DataProvider.Loaders.History.Data {
    // todo implement dense container, check for productivity
    public class HistoryContainer : IHistoryContainer {
        private readonly IStorage<string, DateTime, IHistoryField, string> _data;

        public HistoryContainer(IStorage<string, DateTime, IHistoryField, string> data) {
            _data = data;
        }

        public string Get(string i1, DateTime i2, IHistoryField i3) {
            return _data.Get(i1, i2, i3);
        }

        public bool TryGet(string i1, DateTime i2, IHistoryField i3, out string value) {
            return _data.TryGet(i1, i2, i3, out value);
        }

        public void Set(string i1, DateTime i2, IHistoryField i3, string value) {
            _data.Set(i1, i2, i3, value);
        }

        public IStorage<DateTime, IHistoryField, string> this[string i1] {
            get { return _data[i1]; }
        }

        public IStorage<string, IHistoryField, string> this[DateTime i2] {
            get { return _data[i2]; }
        }

        public string[] Slice1() {
            return _data.Slice1();
        }

        public DateTime[] Slice2() {
            return _data.Slice2();
        }

        public IHistoryField[] Slice3() {
            return _data.Slice3();
        }

        public IStorage<string, DateTime, string> this[IHistoryField i3] {
            get { return _data[i3]; }
        }
    }
}