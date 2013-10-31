using System.Collections.Generic;

namespace DataProvider.Loaders.History.Data {
    public interface IStorage<T1, T2, TValue> {
        TValue Get(T1 i1, T2 i2);
        bool TryGet(T1 i1, T2 i2, out TValue value);
        void Set(T1 i1, T2 i2, TValue value);

        IDictionary<T1, TValue> this[T2 i2] { get; }
        IDictionary<T2, TValue> this[T1 i1] { get; }
    }

    public interface IStorage<T1, T2, T3, TValue> {
        TValue Get(T1 i1, T2 i2, T3 i3);
        bool TryGet(T1 i1, T2 i2, T3 i3, out TValue value);
        void Set(T1 i1, T2 i2, T3 i3, TValue value);

        IStorage<T1, T2, TValue> this[T3 i3] { get; }
        IStorage<T2, T3, TValue> this[T1 i1] { get; }
        IStorage<T1, T3, TValue> this[T2 i2] { get; }
    }
}