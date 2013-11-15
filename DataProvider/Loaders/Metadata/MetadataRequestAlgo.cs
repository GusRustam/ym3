using System;
using System.Linq;
using DataProvider.Loaders.Metadata.Data;
using DataProvider.Objects;
using Dex2;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using Toolbox.Async;

namespace DataProvider.Loaders.Metadata {
    public class MetadataRequestAlgo<T> : TimeoutCall where T : IMetadataItem, new() {
        private readonly IEikonObjects _objects;
        private readonly IRequestSetup<T> _setup;
        private readonly IMetadataContainer<T> _res;
        private readonly IMetadataImporter<T> _importer;

        private Dex2Mgr _dex2Manager;
        private RData _rData;

        public MetadataRequestAlgo(IEikonObjects objects, IMetaObjectFactory<T> factory, ILogger logger, IRequestSetup<T> setup)
            : base(logger) {
            _objects = objects;
            _setup = setup;
            _res = factory.CreateContainer(setup);
            _importer = factory.CreateImporter(setup);
        }

        protected override void Prepare() {
            this.Trace("Prepare()");
            try {
                _dex2Manager = _objects.CreateDex2Mgr();
                var cookie = _dex2Manager.Initialize();
                _rData = _dex2Manager.CreateRData(cookie);
            } catch (Exception e) {
                ReportError(e);
            }
        }

        protected override void Perform() {
            this.Trace("Perform()");
            try {
                _rData.OnUpdate += OnUpdate;
                _rData.InstrumentIDList = string.Join(",", _setup.Rics);
                _rData.FieldList = string.Join(",", _setup.Fields);
                _rData.DisplayParam = _setup.DisplayMode;
                _rData.RequestParam = _setup.RequestMode;
                _rData.Subscribe();
                if (_rData.Data != null)
                    LoadData(_rData.Data);
            } catch (Exception e) {
                ReportError(e);
            }
        }

        private void LoadData(object dt) {
            this.Trace("LoadData()");
            lock (LockObj) {
                var data = (object[,])dt;

                var minRow = data.GetLowerBound(0);
                var maxRow = data.GetUpperBound(0);
                var minCol = data.GetLowerBound(1);
                var maxCol = data.GetUpperBound(1);

                for (var row = minRow; row <= maxRow; row++) {
                    var currentRow = new object[maxCol - minCol + 1];
                    var i = 0;
                    for (var col = minCol; col <= maxCol; col++)
                        currentRow[i++] = data.GetValue(row, col);

                    try {
                        _res.Rows.Add(_importer.Import(currentRow));
                    } catch (Exception e) {
                        this.Warn(string.Format("Failed to import row #{0}, id field {1}", row, currentRow.Any() ? currentRow[0] : "N/A"), e);
                    }
                }
                TryChangeState(State.Succeded);
            }
        }

        private void OnUpdate(DEX2_DataStatus dataStatus, object error) {
            lock (LockObj) {
                switch (dataStatus) {
                    case DEX2_DataStatus.DE_DS_FULL:
                        LoadData(_rData.Data);
                        break;

                    case DEX2_DataStatus.DE_DS_PARTIAL:
                        this.Warn("Partial data!");
                        break;

                    case DEX2_DataStatus.DE_DS_NULL_ERROR:
                    case DEX2_DataStatus.DE_DS_NULL_EMPTY:
                    case DEX2_DataStatus.DE_DS_NULL_TIMEOUT:
                        ReportError(new Exception((error ?? "No data").ToString()));
                        break;

                    default:
                        ReportError(new ArgumentOutOfRangeException("dataStatus"));
                        break;
                }
            }
        }

        protected override void Finish() {
            this.Trace("Finish()");
            lock (LockObj) {
                if (_setup.Callback != null)
                    _setup.Callback(_res);
            }
        }

        protected override void HandleTimout() {
            this.Trace("HandleTimout()");
            // todo
        }

        protected override void HandleError(Exception ex) {
            this.Trace(string.Format("HandleError(), exception is\n{0}", ex));
            // todo
        }

        protected override void HandleCancel() {
            this.Trace("HandleCancel()");
            // todo
        }

        private void ReportError(Exception ex) {
            this.Trace(string.Format("ReportError, error is\n{0}", ex));
            Report = ex;
            // todo _res.Records.Add(new ChainRecord(_ric, TimeoutStatus.CreateError(ex), new List<string>()));
            TryChangeState(State.Invalid);
        }
    }
}