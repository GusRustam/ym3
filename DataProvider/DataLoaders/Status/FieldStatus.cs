using System;
using System.Linq;
using ThomsonReuters.Interop.RTX;

namespace DataProvider.DataLoaders.Status {
    public class FieldStatus : IFieldStatus {
        private readonly string _name;
        public static readonly IFieldStatus Ok = new FieldStatus("Ok");
        public static readonly IFieldStatus Invalid = new FieldStatus("Invalid");
        public static readonly IFieldStatus UnknownOrUndefined = new FieldStatus("UnknownOrUndefined");

        private static readonly IFieldStatus[] Statuses = { Ok, Invalid, UnknownOrUndefined };

        private FieldStatus(string name) {
            _name = name;
        }

        public static IFieldStatus FromAdxStatus(RT_FieldStatus status) {
            switch (status) {
                case RT_FieldStatus.RT_FIELD_OK:
                    return Ok;
                case RT_FieldStatus.RT_FIELD_INVALID:
                    return Invalid;
                case RT_FieldStatus.RT_FIELD_UNKNOWN:
                case RT_FieldStatus.RT_FIELD_UNDEFINED:
                    return UnknownOrUndefined;
                default:
                    return null;
            }
        }

        public static IFieldStatus FromString(string name) {
            var stringStatuses = Statuses.Select(status => status.ToString()).ToArray();
            var pos = Array.IndexOf(stringStatuses, name);
            return pos < 0 ? null : Statuses[pos];
        }

        public override string ToString() {
            return _name;
        }
    }
}