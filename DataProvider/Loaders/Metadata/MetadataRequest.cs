using System;
using DataProvider.Objects;
using Dex2;
using LoggingFacility;
using Toolbox.Async;

namespace DataProvider.Loaders.Metadata {
    public class MetadataRequest<T> : IMetadataRequest<T> where T : IMetadataItem, new() {
        private readonly MetadataRequestAlgo _algo;

        public class MetadataRequestAlgo : TimeoutCall {
            private readonly IEikonObjects _objects;
            private readonly IMetaRequestSetup<T> _setup;
            private Dex2Mgr _dex2Manager;
            private RData _rData;
            private readonly IMetadataContainer<T> _res;

            public MetadataRequestAlgo(IEikonObjects objects, IMetaObjectFactory<T> factory, ILogger logger, IMetaRequestSetup<T> setup) : base(logger) {
                _objects = objects;
                _setup = setup;
                _res = factory.CreateContainer();
            }

            protected override void Prepare() {
                try {
                    _dex2Manager = _objects.CreateDex2Mgr();
                    var cookie = _dex2Manager.Initialize();
                    _rData = _dex2Manager.CreateRData(cookie);
                } catch (Exception e) {
                    ReportError(e);
                }
            }

            protected override void Perform() {
                try {
                    _rData.FieldList = _setup.Fields;
                    _rData.DisplayParam = _setup.DisplayMode;
                    _rData.RequestParam = _setup.RequestMode;
                    _rData.Subscribe(false);
                } catch (Exception e) {
                    ReportError(e);
                }
            }

            protected override void Finish() {
                lock (LockObj) {
                    if (_setup.Callback != null)
                        _setup.Callback(_res);
                }
            }

            protected override void HandleTimout() {
                throw new NotImplementedException();
            }

            protected override void HandleError(Exception ex) {
                throw new NotImplementedException();
            }

            protected override void HandleCancel() {
                throw new NotImplementedException();
            }

            private void ReportError(Exception ex) {
                Report = ex;
                // todo _res.Records.Add(new ChainRecord(_ric, TimeoutStatus.CreateError(ex), new List<string>()));
                TryChangeState(State.Invalid);
            }
        }

        public MetadataRequest(IMetaObjectFactory<T> factory, IMetaRequestSetup<T> setup) {
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