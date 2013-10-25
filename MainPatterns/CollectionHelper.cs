using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Toolbox {
    public static class CollectionHelper {
        public static bool Belongs<T>(this T who, params T[] arr)  {
            return arr.Contains(who);
        }
    }
}
