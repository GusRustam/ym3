using System;
using System.Globalization;
using System.Linq;
using DataProvider.Annotations;
using DataProvider.Loaders.History.Data;
using DataProvider.Loaders.Status;
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

        [UsedImplicitly]
        private class AdxHistoryAlgorithm : TimeoutCall {
            private readonly HistorySetup _setup;
            private readonly string _ric;
            private readonly AdxRtHistory _adxRtHistory;
            private readonly IContainer _container;
            private IHistoryContainer _res;

            public AdxHistoryAlgorithm(IContainer container, IEikonObjects objects, ILogger logger, HistorySetup setup, string ric) 
                : base(logger) {
                _setup = setup;
                _ric = ric;
                _container = container;
                _adxRtHistory = objects.CreateAdxRtHistory();
                this.Trace(string.Format("OnAlgorithm({0})", objects.GetType().Name));
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
                    _adxRtHistory.ItemName = _ric;
                    _adxRtHistory.OnUpdate += OnUpdate;
                    _adxRtHistory.RequestHistory(GetFields());
                } catch (Exception e) {
                    this.Error("Failed", e);
                }
            }

            protected override void Finish() {
                this.Trace("Success()");
                if (_setup.Callback != null)
                    _setup.Callback(_res);
            }

            protected override void HandleTimout() {
                lock (LockObj) {
                    _res.Status = TimeoutStatus.Timeout;
                    _res.RicStatuses[_ric] = TimeoutStatus.Timeout;
                }
            }

            protected override void HandleError(Exception ex) {
                lock (LockObj) {
                    var err = TimeoutStatus.CreateError(ex);
                    _res.Status = err;
                    _res.RicStatuses[_ric] = err;
                }
            }

            protected override void HandleCancel() {
                lock (LockObj) {
                    _res.Status = TimeoutStatus.Cancelled;
                    _res.RicStatuses[_ric] = TimeoutStatus.Cancelled;
                }
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
                this.Trace(string.Format("OnUpdate({0})", DataStatus.FromAdxStatus(dataStatus)));
                lock (LockObj) {
                    try {
                        switch (dataStatus) {
                            case RT_DataStatus.RT_DS_PARTIAL:
                                this.Info("Got partial data!!!");
                                if (!ImportTable()) {
                                    ReportError(new InvalidOperationException("Failed to import"));
                                }
                                break;

                            case RT_DataStatus.RT_DS_FULL:
                                try {
                                    if (!ImportTable()) {
                                        ReportError(new InvalidOperationException("Failed to import"));
                                    } else
                                        _res.RicStatuses[_ric] = TimeoutStatus.Ok;
                                        TryChangeState(State.Succeded);
                                } finally {
                                    _adxRtHistory.FlushData();
                                }
                                break;
                            default:
                                ReportError(new Exception(_adxRtHistory.ErrorString));
                                break;
                        }
                    } catch (Exception e) {
                        this.Error("Failed to update", e);
                    }
                }
            }

            private void ReportError(Exception exception) {
                Report = exception;
                _res.RicStatuses[_ric] = TimeoutStatus.Error;
                TryChangeState(State.Invalid);
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
                            _res.Set(_ric, ricDate, HistoryField.FromAdxName(fieldName), fieldValue);
                        }
                    }
                    _startCol = lastColumn;
                    return true;
                } catch (Exception e) {
                    this.Warn("Failed to parse", e);
                    return false;
                }
            }
        }

        public AdxHistoryRequest(IContainer container, ILogger logger, HistorySetup setup, string ric) {
            _algo = container
                .With(typeof(string), ric)
                .With(setup)
                .GetInstance<AdxHistoryAlgorithm>();
            Logger = logger;
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