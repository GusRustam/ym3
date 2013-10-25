using DataProvider.DataLoaders.Status;

namespace DataProvider.DataLoaders {
    public class RicFields : IRicFields {
        internal RicFields(IItemStatus status, string[] fields) {
            Status = status;
            Fields = fields;
        }

        public RicFields(IItemStatus status) {
            Status = status;
            Fields = new string[0];
        }

        public string[] Fields { get; private set; }
        public IItemStatus Status { get; private set; }
    }
}