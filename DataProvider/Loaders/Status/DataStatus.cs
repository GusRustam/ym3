using System;
using System.Linq;
using ThomsonReuters.Interop.RTX;

namespace DataProvider.Loaders.Status {
    public class DataStatus : IDataStatus {
        private readonly string _name;
        public static readonly IDataStatus Full = new DataStatus("Full");
        public static readonly IDataStatus Partial = new DataStatus("Partial");
        public static readonly IDataStatus Error = new DataStatus("Error");

        private static readonly IDataStatus[] Statuses = { Full, Partial, Error };

        private DataStatus(string name) {
            _name = name;
        }

        public static IDataStatus FromAdxStatus(RT_DataStatus status) {
            switch (status) {
                case RT_DataStatus.RT_DS_FULL:
                    return Full;

                case RT_DataStatus.RT_DS_PARTIAL:
                    return Partial;

                case RT_DataStatus.RT_DS_NULL_ERROR:
                case RT_DataStatus.RT_DS_NULL_EMPTY:
                case RT_DataStatus.RT_DS_NULL_TIMEOUT:
                    return Error;

                default:
                    return null;
            }
        }

        public static IDataStatus FromString(string name) {
            var stringStatuses = Statuses.Select(status => status.ToString()).ToArray();
            var pos = Array.IndexOf(stringStatuses, name);
            return pos < 0 ? null : Statuses[pos];
        }

        public override string ToString() {
            return _name;
        }
    }
}