using System;
using System.Collections;
using System.Collections.Generic;

namespace DataProvider.Storage {
    public abstract class StorageBase<T1, T2, TValue> : IStorage<T1, T2, TValue> {
        public abstract TValue Get(T1 i1, T2 i2);
        public abstract void Set(T1 i1, T2 i2, TValue value);
        public abstract IDictionary<T1, TValue> this[T2 i2] { get; }
        public abstract IDictionary<T2, TValue> this[T1 i1] { get; }
        public abstract T1[] Slice1();
        public abstract T2[] Slice2();

        public bool TryGet(T1 i1, T2 i2, out TValue value) {
            try {
                value = Get(i1, i2);
            } catch (Exception) {
                value = default(TValue);
                return false;
            }
            return true;
        }

        public abstract IEnumerator<KeyValuePair<Tuple<T1, T2>, TValue>> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }

    public abstract class StorageBase<T1, T2, T3, TValue> : IStorage<T1, T2, T3, TValue> {
        public abstract TValue Get(T1 i1, T2 i2, T3 i3);
        public abstract void Set(T1 i1, T2 i2, T3 i3, TValue value);
        public abstract IStorage<T1, T2, TValue> this[T3 i3] { get; }
        public abstract IStorage<T2, T3, TValue> this[T1 i1] { get; }
        public abstract IStorage<T1, T3, TValue> this[T2 i2] { get; }
        public abstract T1[] Slice1();
        public abstract T2[] Slice2();
        public abstract T3[] Slice3();

        public bool TryGet(T1 i1, T2 i2, T3 i3, out TValue value) {
            try {
                value = Get(i1, i2, i3);
            } catch (Exception) {
                value = default(TValue);
                return false;
            }
            return true;
        }

        public abstract IEnumerator<KeyValuePair<Tuple<T1, T2, T3>, TValue>> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}