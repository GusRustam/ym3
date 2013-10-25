using System;
using System.Linq;
using ThomsonReuters.Interop.RTX;

namespace DataProvider.DataLoaders.Status {
    public class SourceStatus : ISourceStatus {
        private readonly string _name;
        public static readonly ISourceStatus Up = new SourceStatus("Up");
        public static readonly ISourceStatus Down = new SourceStatus("Down");
        public static readonly ISourceStatus Unknown = new SourceStatus("Unknown");

        private static readonly ISourceStatus[] Statuses = { Up, Down, Unknown };

        private SourceStatus(string name) {
            _name = name;
        }

        public static ISourceStatus FromAdxStatus(RT_SourceStatus status) {
            switch (status) {
                case RT_SourceStatus.RT_SOURCE_UP:
                    return Up;

                case RT_SourceStatus.RT_SOURCE_DOWN:
                    return Down;

                case RT_SourceStatus.RT_SOURCE_INVALID:
                    return Down;

                case RT_SourceStatus.RT_SOURCE_UNDEFINED:
                case RT_SourceStatus.RT_SOURCE_NOT_SET:
                    return Unknown;

                default:
                    return null;
            }
        }

        public static ISourceStatus FromString(string name) {
            var stringStatuses = Statuses.Select(status => status.ToString()).ToArray();
            var pos = Array.IndexOf(stringStatuses, name);
            return pos < 0 ? null : Statuses[pos];
        }

        public override string ToString() {
            return _name;
        }
    }
}