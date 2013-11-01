using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Toolbox {
    public static class CollectionHelper {
        public static bool Belongs<T>(this T who, params T[] arr)  {
            return arr.Contains(who);
        }

        public static T[] ToSomeArray<T>(this IEnumerable<T> data) {
            return data != null ? data.ToArray() : new T[0];
        }

        public static IList<T> ToSomeList<T>(this IEnumerable<T> data) {
            return data != null ? data.ToList() : new List<T>();
        }

        public static string ToReutersDate(this DateTime date) {
            return string.Format("{0:ddMMMyyyy}", date);
        }

    }
}
