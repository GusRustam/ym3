using System;
using System.Linq;
using ThomsonReuters.Interop.RTX;

namespace DataProvider.Loaders.Status {
    public class ItemStatus : IItemStatus {
        private readonly string _name;

        public static readonly IItemStatus Ok = new ItemStatus("Ok");
        public static readonly IItemStatus UnknownOrInvalid = new ItemStatus("UnknownOrInvalid");
        public static readonly IItemStatus NotPermissioned = new ItemStatus("NotPermissioned");
        public static readonly IItemStatus Delayed = new ItemStatus("Delayed");

        private static readonly IItemStatus[] Statuses = { Ok, UnknownOrInvalid, NotPermissioned, Delayed };

        public static IItemStatus FromAdxStatus(RT_ItemStatus status) {
            switch (status) {
                case RT_ItemStatus.RT_ITEM_OK:
                    return Ok;

                case RT_ItemStatus.RT_ITEM_INVALID:
                case RT_ItemStatus.RT_ITEM_UNKNOWN:
                case RT_ItemStatus.RT_ITEM_STALE:
                    return UnknownOrInvalid;

                case RT_ItemStatus.RT_ITEM_DELAYED:
                    return Delayed;

                case RT_ItemStatus.RT_ITEM_NOT_PERMISSIONED:
                    return NotPermissioned;

                default:
                    return null;
            }
        }

        public static IItemStatus FromString(string name) {
            var stringStatuses = Statuses.Select(status => status.ToString()).ToArray();
            var pos = Array.IndexOf(stringStatuses, name);
            return pos < 0 ? null : Statuses[pos];
        }

        private ItemStatus(string name) {
            _name = name;
        }

        public override string ToString() {
            return _name;
        }
    }
}