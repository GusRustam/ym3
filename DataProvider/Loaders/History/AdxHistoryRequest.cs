using System;
using DataProvider.Objects;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using StructureMap;
using ThomsonReuters.Interop.RTX;
using Toolbox.Async;

namespace DataProvider.Loaders.History {
    public sealed class AdxHistoryRequest : IHistoryRequest, ISupportsLogging {
        private readonly AdxHistoryAlgorithm _algo;

        private class AdxHistoryAlgorithm : TimeoutCall, ISupportsLogging {
            private readonly HistorySetup _setup;
            private readonly AdxRtHistory _adxRtHistory;

            public AdxHistoryAlgorithm(AdxRtHistory adxRtHistory, ILogger logger, HistorySetup setup) {
                _setup = setup;
                _adxRtHistory = adxRtHistory;
                Logger = logger;
            }

            protected override void Prepare() {
                // nothing to prepare
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
                    _setup.Callback();
            }

            private object[] GetFields() {
                throw new NotImplementedException();
            }

            private string GetModeString() {
                throw new NotImplementedException();
            }

            private void OnUpdate(RT_DataStatus dataStatus) {
                // todo parse and extract
            }

            public ILogger Logger { get; private set; }
        }


        public AdxHistoryRequest(IContainer container, ILogger logger, HistorySetup setup) {
            _algo = new AdxHistoryAlgorithm(container.GetInstance<IEikonObjects>().CreateAdxRtHistory(), logger, setup);
        }

        public void Start() {
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

        public ILogger Logger { get; protected set; }
    }
}