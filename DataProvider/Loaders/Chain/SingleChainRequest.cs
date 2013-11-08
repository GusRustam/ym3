using System;
using System.Collections.Generic;
using DataProvider.Loaders.Status;
using DataProvider.Objects;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using StructureMap;
using ThomsonReuters.Interop.RTX;
using Toolbox.Async;

namespace DataProvider.Loaders.Chain {
    public class SingleChainRequest : IChainRequest {
        private readonly SingleChainAlgo _algo;

        public class SingleChainAlgo : TimeoutCall, ISupportsLogging {
            public ILogger Logger { get; private set; }

            private readonly AdxRtChain _adxRtChain;
            private readonly ChainSetup _setup;
            private readonly string _ric;
            private readonly IChainData _res;

            public SingleChainAlgo(IContainer container, IEikonObjects objects, ChainSetup setup, string ric) {
                Logger = container.GetInstance<ILogger>();
                _adxRtChain = objects.CreateAdxRtChain();
                _res = container.GetInstance<IChainData>();
                _setup = setup;
                _ric = ric;
            }

            protected override void Prepare() {
                _adxRtChain.ErrorMode = AdxErrorMode.EXCEPTION;
                _adxRtChain.Source = _setup.Feed;
                _adxRtChain.ItemName = _ric;
                _adxRtChain.Mode = _setup.Mode;
            }

            protected override void Perform() {
                _adxRtChain.OnStatusChange += OnStatusUpdate;
                _adxRtChain.OnUpdate += OnUpdate;
                _adxRtChain.RequestChain();
            }

            private void OnUpdate(RT_DataStatus dataStatus) {
                lock (LockObj) {
                    var status = DataStatus.FromAdxStatus(dataStatus);
                    if (status == DataStatus.Full) {
                        this.Trace(string.Format("Got full data on ric {0}", _ric));
                        var data = (object[])_adxRtChain.Data;
                        var res = new List<string>();
                        for (var i = data.GetLowerBound(0); i <= data.GetUpperBound(0); i++) {
                            var item = data.GetValue(i).ToString().Trim();
                            if (!string.IsNullOrEmpty(item)) res.Add(item);
                        }
                        _res.ChainRic = _ric;
                        _res.Rics = res;
                        TryChangeState(State.Succeded);
                    } else if (status == DataStatus.Partial) {
                        this.Info(string.Format("Got partial data on ric {0}", _ric));
                    } else {
                        Report = new Exception("Invalid ric");
                        TryChangeState(State.Invalid);
                    }
                }   
            }

            private void OnStatusUpdate(RT_SourceStatus sourceStatus) {
                lock (LockObj) {
                    var status = SourceStatus.FromAdxStatus(sourceStatus);
                    if (status == SourceStatus.Up) return;

                    Report = new Exception("Invalid source");
                    TryChangeState(State.Invalid);
                }
            }

            protected override void Success() {
                if (_setup.Callback != null)
                    _setup.Callback(_res);
            }
        }

        public SingleChainRequest(IContainer container, ChainSetup setup) {
            _algo = container
                .With(setup)
                .With(typeof(string), setup.Rics[0]) // todo ugly too
                .GetInstance<SingleChainAlgo>();
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
            _algo.Request();
        }

        public void Cancel() {
            _algo.Cancel();
        }

    }
}