//using System.Globalization;
using System;
using System.Collections.Generic;

namespace DataProvider.Storage {
    //public interface IConvertibleFromString<out T> {
    //    T FromString(string value);
    //}

    //public class RDouble : IConvertibleFromString<double?> {
    //    public double? FromString(string value) {
    //        double res;
    //        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out res)) return res;
    //        return null;
    //    }
    //}

    public interface IStorage<T1, T2, TValue> : IEnumerable<KeyValuePair<Tuple<T1, T2>, TValue>> {
        TValue Get(T1 i1, T2 i2);
        bool TryGet(T1 i1, T2 i2, out TValue value);
        void Set(T1 i1, T2 i2, TValue value);

        IDictionary<T1, TValue> this[T2 i2] { get; }
        IDictionary<T2, TValue> this[T1 i1] { get; }

        T1[] Slice1();
        T2[] Slice2();
    }

    public interface IStorage<T1, T2, T3, TValue> : IEnumerable<KeyValuePair<Tuple<T1, T2, T3>, TValue>> {
        TValue Get(T1 i1, T2 i2, T3 i3);
        bool TryGet(T1 i1, T2 i2, T3 i3, out TValue value);
        void Set(T1 i1, T2 i2, T3 i3, TValue value);

        IStorage<T1, T2, TValue> this[T3 i3] { get; }
        IStorage<T2, T3, TValue> this[T1 i1] { get; }
        IStorage<T1, T3, TValue> this[T2 i2] { get; }

        T1[] Slice1();
        T2[] Slice2();
        T3[] Slice3();
    }
}