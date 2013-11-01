using System;
using System.Globalization;
using System.Linq;
using DataProvider.Loaders.History.Data;
using DataProvider.Objects;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using StructureMap;
using ThomsonReuters.Interop.RTX;
using Toolbox;
using Toolbox.Async;

namespace DataProvider.Loaders.History {
    public sealed class AdxHistoryRequest : IHistoryRequest, ISupportsLogging {
        private readonly AdxHistoryAlgorithm _algo;

        private class AdxHistoryAlgorithm : TimeoutCall, ISupportsLogging {
            private readonly HistorySetup _setup;
            private readonly AdxRtHistory _adxRtHistory;
            private readonly IContainer _container;
            private IHistoryContainer _res;

            public AdxHistoryAlgorithm(IContainer container, HistorySetup setup) {
                _setup = setup;
                _container = container;
                _adxRtHistory = container.GetInstance<IEikonObjects>().CreateAdxRtHistory();
                Logger = container.GetInstance<ILogger>();
            }

            protected override void Prepare() {
                this.Trace("Prepare()");
                _res = _container.GetInstance<IHistoryContainer>();
            }

            protected override void Perform() {
                this.Trace("Perform()");
                try {
                    _adxRtHistory.ErrorMode = AdxErrorMode.EXCEPTION;
                    _adxRtHistory.Source = _setup.Feed;
                    _adxRtHistory.Mode = GetModeString();
                    _adxRtHistory.ItemName = _setup.Ric;
                    _adxRtHistory.OnUpdate += OnUpdate;
                    _adxRtHistory.RequestHistory(GetFields());
                } catch (Exception e) {
                    this.Error("Failed", e);
                }
            }

            protected override void Success() {
                this.Trace("Success()");
                if (_setup.Callback != null)
                    _setup.Callback(_res);
            }

            private string GetFields() {
                this.Trace("GetFields()");
                var z = string.Join(",", _setup.Fields.Select(x => x.AdxName));
                z = string.Format("{0}, {1}", HistoryField.Date.AdxName, z);
                return z;
            }

            private string GetModeString() {
                this.Trace("GetModeString()");
                var res = "HEADER:YES NULL:SKIP";

                if (_setup.Since.HasValue)
                    JoinStr("START", _setup.Since.Value.ToReutersDate(), ref res);

                if (_setup.Till.HasValue)
                    JoinStr("END", _setup.Till.Value.ToReutersDate(), ref res);

                if (_setup.Rows.HasValue)
                    JoinStr("NBEVENTS", _setup.Rows.Value.ToString(CultureInfo.InvariantCulture), ref res);

                return res;
            }

            private static void JoinStr(string name, string str, ref string res) {
                res = string.IsNullOrEmpty(res) ? str : string.Format("{0} {1}:{2}", res, name, str);
            }

            private void OnUpdate(RT_DataStatus dataStatus) {
                this.Trace("OnUpdate()");
                lock (LockObj) {
                    switch (dataStatus) {
                        case RT_DataStatus.RT_DS_PARTIAL:
                            this.Info("Got partial data!!!");
                            if (!ImportTable()) {
                                Report = new InvalidOperationException("Failed to import");
                                TryChangeState(State.Invalid);
                            }
                            break;

                        case RT_DataStatus.RT_DS_FULL:
                            try {
                                if (!ImportTable()) {
                                    Report = new InvalidOperationException("Failed to import");
                                    TryChangeState(State.Invalid);
                                } else
                                    TryChangeState(State.Succeded);
                            } finally {
                                _adxRtHistory.FlushData();
                            }
                            break;
                        default:
                            TryChangeState(State.Invalid);
                            break;
                    }
                }
            }

            private int _startCol;
            private bool ImportTable() {
                try {
                    this.Trace("ImportTable()");
                    object[,] data = _adxRtHistory.Data;

                    var firstRow = data.GetLowerBound(0);
                    var firstColumn = Math.Max(data.GetLowerBound(1), _startCol);
                    var lastRow = data.GetUpperBound(0);
                    var lastColumn = data.GetUpperBound(1);

                    var fields = new IHistoryField[lastRow - firstRow + 1];
                    for (var row = firstRow; row <= lastRow; row++) {
                        var fieldName = data.GetValue(row, 0).ToString();
                        fields[row] = HistoryField.FromAdxName(fieldName);
                    }

                    for (var col = firstColumn+1; col <= lastColumn; col++) {
                        var dateValue = data.GetValue(0, col).ToString();
                        DateTime ricDate;
                        if (!DateTime.TryParse(dateValue, out ricDate)) continue;

                        this.Trace(string.Format(" -> date {0:dd/MMM/yyyy}", ricDate));
                        for (var row = firstRow; row <= lastRow; row++) {
                            var fieldName = data.GetValue(row, 0).ToString();
                            var fieldValue = data.GetValue(row, col).ToString();
                            this.Trace(string.Format(" -> -> field {0} value {1}", fieldName, fieldValue));
                            _res.Set(_setup.Ric, ricDate, HistoryField.FromAdxName(fieldName), fieldValue);
                        }
                    }
                    _startCol = lastColumn;
                    return true;
                } catch (Exception e) {
                    this.Warn("Failed to parse", e);
                    return false;
                }
            }

            public ILogger Logger { get; private set; }
        }

        public AdxHistoryRequest(IContainer container, HistorySetup setup) {
            _algo = new AdxHistoryAlgorithm(container, setup);
            Logger = container.GetInstance<ILogger>();
        }

        public ITimeoutCall WithCancelCallback(Action callback) {
            _algo.WithCancelCallback(callback);
            return this;
        }

        public ITimeoutCall WithTimeoutCallback(Action callback) {
            _algo.WithTimeoutCallback(callback);
            return this;
        }

        public ITimeoutCall WithErrorCallback(Action<Exception> callback) {
            _algo.WithErrorCallback(callback);
            return this;
        }

        public ITimeoutCall WithTimeout(TimeSpan? timeout) {
            _algo.WithTimeout(timeout);
            return this;
        }

        public void Request() {
            this.Info("Request()");
            _algo.Request();
        }

        public void Cancel() {
            _algo.Cancel();
        }

        public ILogger Logger { get; private set; }
    }
}