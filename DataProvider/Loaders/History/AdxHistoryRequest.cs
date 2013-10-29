using System;
using System.Linq;
using DataProvider.Loaders.History.Data;
using DataProvider.Objects;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using ThomsonReuters.Interop.RTX;

namespace DataProvider.Loaders.History {
    public class AdxHistoryRequest : IHistoryRequest, ISupportsLogging {
        private readonly string _ric;
        private string _feed = "IDN";
        private HistoryInterval _interval = HistoryInterval.Day;
        private DateTime? _since;
        private DateTime? _till;
        private int? _rows;
        private Action<IHistorySnapshot> _callback;
        private HistoryMode _mode = HistoryMode.TimeSales;

        private readonly AdxRtHistory _adxRtHistory;

        public AdxHistoryRequest(IEikonObjects objects, ILogger logger, string ric) {
            _ric = ric;
            _adxRtHistory = objects.CreateAdxRtHistory();
            Logger = logger;
        }

        public IHistoryRequest WithFeed(string feed) {
            _feed = feed;
            return this;
        }

        public IHistoryRequest WithInterval(HistoryInterval interval) {
            _interval = interval;
            return this;
        }

        public IHistoryRequest WithSince(DateTime date) {
            _since = date;
            return this;
        }

        public IHistoryRequest WithTill(DateTime date) {
            _till = date;
            return this;
        }

        public IHistoryRequest WithNumRecords(int num) {
            _rows = num;
            return this;
        }

        public IHistoryRequest WithCallback(Action<IHistorySnapshot> callback) {
            _callback = callback;
            return this;
        }

        public IHistoryRequest WithMode(HistoryMode mode) {
            _mode = mode;
            return this;
        }

        public void Request() {
            Validate();

            var fields = GetFields();

            _adxRtHistory.ErrorMode = AdxErrorMode.EXCEPTION;
            _adxRtHistory.Source = _feed;
            _adxRtHistory.Mode = CompileModeString();
            _adxRtHistory.ItemName = _ric;
            _adxRtHistory.OnUpdate += OnUpdate;
            _adxRtHistory.RequestHistory(fields);
        }

        private void OnUpdate(RT_DataStatus dataStatus) {
            if (_callback != null) _callback();
        }

        private object[] GetFields() {
            switch (_mode) {
                case HistoryMode.TimeSales:
                    break;
                case HistoryMode.TimeSeries:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string CompileModeString() {
            throw new NotImplementedException();
        }

        private void Validate() {
            if (string.IsNullOrEmpty(_feed))
                throw new ArgumentException("feed");

            var elems = new object[] {_since, _till, _rows};
            var countNulls = elems.Count(o => o == null);

            if (countNulls == 0)
                throw new ArgumentException("since, till and nbrows together");
            if (countNulls >= 2)
                throw new ArgumentException("insufficient since, till and nbrows data");

        }

        public ILogger Logger { get; private set; }
    }
}