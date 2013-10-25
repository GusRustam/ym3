using System;
using System.Linq;

namespace LoggingFacility {
    public class Level : IEquatable<Level> {
        private readonly int _ord;
        private readonly string _name;

        public static readonly Level Trace = new Level(1, "Trace");
        public static readonly Level Debug = new Level(2, "Debug");
        public static readonly Level Info = new Level(3, "Info");
        public static readonly Level Warn = new Level(4, "Warn");
        public static readonly Level Error = new Level(5, "Error");
        public static readonly Level Fatal = new Level(6, "Fatal");
        public static readonly Level Off = new Level(7, "Off");

        private static readonly Level[] AllLevels = { Trace, Debug, Info, Warn, Error, Fatal, Off };

        public static Level FromString(string level) {
            return AllLevels.FirstOrDefault(lvl => lvl.ToString() == level);
        }

        public override string ToString() {
            return _name;
        }

        public bool Equals(Level other) {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return _ord == other._ord;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((Level)obj);
        }

        public override int GetHashCode() {
            return _ord;
        }

        public static bool operator ==(Level left, Level right) {
            return Equals(left, right);
        }

        public static bool operator !=(Level left, Level right) {
            return !Equals(left, right);
        }

        public static bool operator >(Level l1, Level l2) {
            if (l1 == null || l2 == null)
                return false;
            return l1._ord > l2._ord;
        }

        public static bool operator >=(Level l1, Level l2) {
            if (l1 == null || l2 == null)
                return false;
            return l1._ord >= l2._ord;
        }

        public static bool operator <=(Level l1, Level l2) {
            if (l1 == null || l2 == null)
                return false;
            return !(l1 > l2);
        }

        public static bool operator <(Level l1, Level l2) {
            if (l1 == null || l2 == null)
                return false;
            return !(l1 >= l2);
        }

        private Level(int ord, string name) {
            _ord = ord;
            _name = name;
        }
    }
}
