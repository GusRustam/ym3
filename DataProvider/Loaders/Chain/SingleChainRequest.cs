using System;
using System.Collections.Generic;
using System.Linq;
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

            public SingleChainAlgo(IContainer container, IEikonObjects objects, ILogger logger, ChainSetup setup, string ric) {
                Logger = logger;
                _adxRtChain = objects.CreateAdxRtChain();
                _res = container.GetInstance<IChainData>();
                _setup = setup;
                _ric = ric;
            }

            protected override void Prepare() {
                try {
                    _adxRtChain.ErrorMode = AdxErrorMode.EXCEPTION;
                    _adxRtChain.Source = _setup.Feed;
                    _adxRtChain.ItemName = _ric;
                    _adxRtChain.Mode = _setup.Mode;
                } catch (Exception e) {
                    this.Trace("Error in Prepare", e);
                    Report = e;
                    TryChangeState(State.Invalid);
                }
            }

            protected override void Perform() {
                try {
                    _adxRtChain.OnStatusChange += OnStatusUpdate;
                    _adxRtChain.OnUpdate += OnUpdate;

                    _adxRtChain.RequestChain();
                } catch (Exception e) {
                    this.Trace("Error in Perform", e);
                    Report = e;
                    TryChangeState(State.Invalid);
                }
            }

            private void OnUpdate(RT_DataStatus dataStatus) {
                this.Trace("OnUpdate()");
                lock (LockObj) {
                    var status = DataStatus.FromAdxStatus(dataStatus);
                    if (status == DataStatus.Full) {
                        this.Trace(string.Format("Got full data on ric {0}", _ric));
                        try {
                            var data = (Array)(object)_adxRtChain.Data;
                            var response = data
                                .Cast<object>()
                                .Where(item => item != null)
                                .Select(item => item.ToString().Trim())
                                .Where(item => !string.IsNullOrEmpty(item))
                                .ToList();
                            
                            _res.Data.Add(_ric, response);
                            this.Trace(string.Format("Imported data ot ric {0} successfully", _ric));
                            TryChangeState(State.Succeded);
                        } catch (Exception e) {
                            Report = e;
                            TryChangeState(State.Invalid);
                        }
                    } else if (status == DataStatus.Partial) {
                        this.Info(string.Format("Got partial data on ric {0}", _ric));
                    } else {
                        Report = new Exception("Invalid ric");
                        TryChangeState(State.Invalid);
                    }
                }   
            }

            private void OnStatusUpdate(RT_SourceStatus sourceStatus) {
                this.Trace(string.Format("OnStatusUpdate({0})", SourceStatus.FromAdxStatus(sourceStatus)));
                lock (LockObj) {
                    var status = SourceStatus.FromAdxStatus(sourceStatus);
                    if (status == SourceStatus.Up) return;

                    Report = new Exception("Invalid source");
                    TryChangeState(State.Invalid);
                }
            }

            protected override void Success() {
                lock (LockObj) {
                    this.Trace(string.Format("Success(rics: {0})", _res.Data.Keys.Count));
                    if (_setup.Callback != null)
                        _setup.Callback(_res);
                }
            }
        }

        public SingleChainRequest(IContainer container, ChainSetup setup) {
            Rics = new List<string>(setup.Rics);
            _algo = container
                .With(setup)
                .With(typeof(string), setup.Rics[0]) // todo a bit ugly
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

        public IList<string> Rics { get; private set; }
    }
}