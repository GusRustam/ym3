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
                _res = _container.GetInstance<HistoryContainer>();
            }

            protected override void Perform() {
                _adxRtHistory.ErrorMode = AdxErrorMode.EXCEPTION;
                _adxRtHistory.Source = _setup.Feed;
                _adxRtHistory.Mode = GetModeString();
                _adxRtHistory.ItemName = _setup.Ric;
                _adxRtHistory.OnUpdate += OnUpdate;
                _adxRtHistory.RequestHistory(GetFields());
            }

            protected override void Success() {
                if (_setup.Callback != null)
                    _setup.Callback(_res);
            }

            private object[] GetFields() {
                return _setup.Fields.Select(x => (object)x.AdxName).ToArray();
            }

            private string GetModeString() {
                var res = "HEADER:YES";

                if (_setup.Since.HasValue)
                    JoinStr(_setup.Since.Value.ToReutersDate(), ref res);

                if (_setup.Till.HasValue)
                    JoinStr(_setup.Till.Value.ToReutersDate(), ref res);

                if (_setup.Rows.HasValue)
                    JoinStr(_setup.Rows.Value.ToString(CultureInfo.InvariantCulture), ref res);

                return res;
            }

            private static void JoinStr(string str, ref string res) {
                res = string.IsNullOrEmpty(res) ? str : string.Format("{0} {1}", res, str);
            }

            private void OnUpdate(RT_DataStatus dataStatus) {
                lock (LockObj) {
                    switch (dataStatus) {
                        case RT_DataStatus.RT_DS_FULL:
                            try {
                                object[,] data = _adxRtHistory.Data;

                                var firstRow = data.GetLowerBound(0);
                                var firstColumn = data.GetLowerBound(1);
                                var lastRow = data.GetUpperBound(0);
                                var lastColumn = data.GetUpperBound(1);

                                for (var col = firstColumn; col <= lastColumn; col++) {
                                    var dateValue = data.GetValue(0, col).ToString();
                                    DateTime ricDate;
                                    if (!DateTime.TryParse(dateValue, out ricDate)) continue;

                                    for (var row = firstRow; row <= lastRow; row++) {
                                            var fieldName = data.GetValue(row, 0).ToString();
                                            var fieldValue = data.GetValue(row, col).ToString();
                                            _res.Set(_setup.Ric, ricDate, HistoryField.FromAdxName(fieldName), fieldValue);
                                        }
                                    }
                                TryChangeState(State.Succeded);
                            } catch (Exception ex) {
                                this.Warn("Failed to parse", ex);
                            } finally {
                                _adxRtHistory.FlushData();
                            }
                            break;
                        case RT_DataStatus.RT_DS_PARTIAL:
                            this.Info("Got partial data!!!");
                            break;
                        default:
                            TryChangeState(State.Invalid);
                            break;
                    }
                }
            }

            public ILogger Logger { get; private set; }
        }


        public AdxHistoryRequest(IContainer container, ILogger logger, HistorySetup setup) {
            _algo = new AdxHistoryAlgorithm(container, setup);
            Logger = logger;
        }

        public ITimeoutCall WithCallback(Action callback) {
            _algo.WithCallback(callback);
            return this;
        }

        public ITimeoutCall WithTimeout(TimeSpan? timeout) {
            _algo.WithTimeout(timeout);
            return this;
        }

        public void Request() {
            _algo.Request();
        }

        public ILogger Logger { get; private set; }
    }
}