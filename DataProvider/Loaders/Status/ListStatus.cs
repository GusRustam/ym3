using System;
using System.Linq;
using ThomsonReuters.Interop.RTX;

namespace DataProvider.Loaders.Status {
    public class ListStatus : IListStatus {
        private readonly string _name;
        public static readonly IListStatus Inactive = new ListStatus("Inactive");
        public static readonly IListStatus Running = new ListStatus("Running");

        private static readonly IListStatus[] Statuses = { Inactive, Running };

        private ListStatus(string name) {
            _name = name;
        }

        public static IListStatus FromAdxStatus(RT_ListStatus status) {
            switch (status) {
                case RT_ListStatus.RT_LIST_INACTIVE:
                    return Inactive;

                case RT_ListStatus.RT_LIST_RUNNING:
                    return Running;

                case RT_ListStatus.RT_LIST_UPDATES_STOPPED:
                case RT_ListStatus.RT_LIST_LINKS_CLOSED:
                    return Inactive;

                default:
                    return null;
            }
        }

        public static IListStatus FromString(string name) {
            var stringStatuses = Statuses.Select(status => status.ToString()).ToArray();
            var pos = Array.IndexOf(stringStatuses, name);
            return pos < 0 ? null : Statuses[pos];
        }

        public override string ToString() {
            return _name;
        }
    }
}