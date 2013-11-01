using System;
using Interop.TSI6;

namespace DataProvider.Loaders.History.Data {
    public class HistoryField : IHistoryField {
        private readonly string _adxName;
        private readonly string _tsiName;
        public static IHistoryField Bid = new HistoryField("Bid", "BID", "BID");
        public static IHistoryField Ask = new HistoryField("Ask", "ASK", "ASK");
        public static IHistoryField Close = new HistoryField("Close", "CLOSE", "CLOSE");
        public static IHistoryField VWAP = new HistoryField("VWAP", "VWAP", "VWAP");
        public static IHistoryField Value = new HistoryField("Value", "VALUE", "VALUE");
        public static IHistoryField Volume = new HistoryField("Volume", "VOLUMNE", "VOLUME");
        //public static IHistoryField Bid = new HistoryField("Bid", "BID", TsiFactNames.tsiTsFactBID);
        //public static IHistoryField Ask = new HistoryField("Ask", "ASK", TsiFactNames.tsiTsFactASK);
        //public static IHistoryField Close = new HistoryField("Close", "CLOSE", TsiFactNames.tsiTsFactCLOSE);
        //public static IHistoryField VWAP = new HistoryField("VWAP", "VWAP", TsiFactNames.tsiTsFactVWAP);
        //public static IHistoryField Value = new HistoryField("Value", "VALUE", TsiFactNames.tsiTsFactVALUE);
        //public static IHistoryField Volume = new HistoryField("Volume", "VOLUMNE", TsiFactNames.tsiTsFactVOLUME);

        private HistoryField(string name, string adxName, string tsiName) {
            _adxName = adxName;
            _tsiName = tsiName;
            Name = name;
        }

        public string Name { get; private set; }
        public string TsiName { get { return _tsiName;  } }
        public string AdxName { get { return _adxName; } }

        public static IHistoryField FromAdxName(string name) {
            throw new NotImplementedException();
        }
        public static IHistoryField FromTsiName(string name) {
            throw new NotImplementedException();
        }
    }
}