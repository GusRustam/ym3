using System;
using DataProvider.Objects;
using Dex2;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using Toolbox.Async;

namespace DataProvider.Loaders.Metadata {
    public class MetadataRequest<T> : IMetadataRequest<T> where T : IMetadataItem, new() {
        private readonly MetadataRequestAlgo _algo;

        public class MetadataRequestAlgo : TimeoutCall {
            private readonly IEikonObjects _objects;
            private readonly IRequestSetup<T> _setup;
            private Dex2Mgr _dex2Manager;
            private RData _rData;
            private readonly IMetadataContainer<T> _res;

            public MetadataRequestAlgo(IEikonObjects objects, IMetaObjectFactory<T> factory, ILogger logger, IRequestSetup<T> setup) : base(logger) {
                _objects = objects;
                _setup = setup;
                _res = factory.CreateContainer(setup);
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
                    _rData.Subscribe(false);
                } catch (Exception e) {
                    ReportError(e);
                }
            }

            private void OnUpdate(DEX2_DataStatus dataStatus, object error) {
                lock (LockObj) {
                    switch (dataStatus) {
                        case DEX2_DataStatus.DE_DS_FULL:
                            this.Trace("Full data!");
                            var data = (object[,])_rData.Data;

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
                                    // todo 1) Import row currently not implemented
                                    // todo 2) I do not know (for sure) to which ric does it correspond
                                    // todo ---> I can use first column as ID - well, why not.
                                    // todo ---> yes, it gonna be a convention
                                    // todo 3) so, I can collect invalid IDs (i don't write rics since it could be ISINs)
                                    // todo and then report them
                                    _res.ImportRow(currentRow);
                                } catch (Exception e) {
                                    this.Warn(string.Format("Failed to import row #{0}", row), e);
                                    // todo currentRow[0] -> ID, report this id
                                }
                            }
                            TryChangeState(State.Succeded);
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
            }

            protected override void HandleError(Exception ex) {
                this.Trace(string.Format("HandleError(), exception is\n{0}", ex));
            }

            protected override void HandleCancel() {
                this.Trace("HandleCancel()");
            }

            private void ReportError(Exception ex) {
                this.Trace(string.Format("ReportError, error is\n{0}", ex));
                Report = ex;
                // todo _res.Records.Add(new ChainRecord(_ric, TimeoutStatus.CreateError(ex), new List<string>()));
                TryChangeState(State.Invalid);
            }
        }

        public MetadataRequest(IMetaObjectFactory<T> factory, IRequestSetup<T> setup) {
            _algo = factory.CreateAlgo(setup);
        }

        public ITimeoutCall WithTimeout(TimeSpan? timeout) {
            _algo.WithTimeout(timeout);
            return this;
        }

        public void Request() {
            _algo.Request();
        }

        public void Cancel() {
            _algo.Cancel();
        }
    }
}