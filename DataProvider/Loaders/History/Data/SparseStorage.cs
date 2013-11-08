using System;
using System.Collections.Generic;
using System.Linq;

namespace DataProvider.Loaders.History.Data {
    public class SparseStorage<T1, T2, TValue> : StorageBase<T1, T2, TValue> {
        private readonly IDictionary<Tuple<T1, T2>, TValue> _data = new Dictionary<Tuple<T1, T2>, TValue>();
        public override TValue Get(T1 i1, T2 i2) {
            return _data[Tuple.Create(i1, i2)];
        }

        public override void Set(T1 i1, T2 i2, TValue value) {
            _data[Tuple.Create(i1, i2)] = value;
        }

        public override IDictionary<T1, TValue> this[T2 i2] {
            get {
                var res = new Dictionary<T1, TValue>();
                foreach (var key in _data.Keys.Where(key => key.Item2.Equals(i2)))
                    res[key.Item1] = _data[key];
                return res;
            }
        }

        public override IDictionary<T2, TValue> this[T1 i1] {
            get {
                var res = new Dictionary<T2, TValue>();
                foreach (var key in _data.Keys.Where(key => key.Item1.Equals(i1)))
                    res[key.Item2] = _data[key];
                return res;
            }
        }

        public override T1[] Slice1() {
            var res = new HashSet<T1>();
            foreach (var value in _data.Keys)
                res.Add(value.Item1);
            return res.ToArray();
        }

        public override T2[] Slice2() {
            var res = new HashSet<T2>();
            foreach (var value in _data.Keys)
                res.Add(value.Item2);
            return res.ToArray();
        }

        public override IEnumerator<KeyValuePair<Tuple<T1, T2>, TValue>> GetEnumerator() {
            return _data.GetEnumerator();
        }
    }

    public class SparseStorage<T1, T2, T3, TValue> : StorageBase<T1, T2, T3, TValue> {
        private readonly IDictionary<Tuple<T1, T2, T3>, TValue> _data = new Dictionary<Tuple<T1, T2, T3>, TValue>();
        public override TValue Get(T1 i1, T2 i2, T3 i3) {
            return _data[Tuple.Create(i1, i2, i3)];
        }

        public override void Set(T1 i1, T2 i2, T3 i3, TValue value) {
            _data[Tuple.Create(i1, i2, i3)] = value;
        }

        public override IStorage<T1, T2, TValue> this[T3 i3] {
            get {
                var res = new SparseStorage<T1, T2, TValue>();
                foreach (var key in _data.Keys.Where(key => key.Item3.Equals(i3)))
                    res.Set(key.Item1, key.Item2, _data[key]);
                return res;
            }
        }

        public override IStorage<T2, T3, TValue> this[T1 i1] {
            get {
                var res = new SparseStorage<T2, T3, TValue>();
                foreach (var key in _data.Keys.Where(key => key.Item1.Equals(i1)))
                    res.Set(key.Item2, key.Item3, _data[key]);
                return res;
            }
        }

        public override IStorage<T1, T3, TValue> this[T2 i2] {
            get {
                var res = new SparseStorage<T1, T3, TValue>();
                foreach (var key in _data.Keys.Where(key => key.Item2.Equals(i2)))
                    res.Set(key.Item1, key.Item3, _data[key]);
                return res;
            }
        }

        public override T1[] Slice1() {
            var res = new HashSet<T1>();
            foreach (var value in _data.Keys) 
                res.Add(value.Item1);
            return res.ToArray();
        }

        public override T2[] Slice2() {
            var res = new HashSet<T2>();
            foreach (var value in _data.Keys)
                res.Add(value.Item2);
            return res.ToArray();
        }

        public override T3[] Slice3() {
            var res = new HashSet<T3>();
            foreach (var value in _data.Keys)
                res.Add(value.Item3);
            return res.ToArray();
        }

        public override IEnumerator<KeyValuePair<Tuple<T1, T2, T3>, TValue>> GetEnumerator() {
            return _data.GetEnumerator();
        }
    }
}