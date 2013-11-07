using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataProvider.Annotations;
using DataProvider.Loaders.History.Data;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using StructureMap;
using Toolbox.Async;

namespace DataProvider.Loaders.History {
    class MultiHistoryRequest : IHistoryRequest, ISupportsLogging  {
        private readonly MultuHistoryAlgorithm _algo;

        [UsedImplicitly]
        private class MultuHistoryAlgorithm : TimeoutCall, ISupportsLogging {
            private readonly IContainer _container;
            private readonly HistorySetup _setup;
            private readonly IEnumerable<string> _rics;
            private IHistory _loader;
            private IHistoryContainer _res;
            private readonly IDictionary<string, IHistoryRequest> _subscriptions;

            public MultuHistoryAlgorithm(IContainer container, ILogger logger, HistorySetup setup, IEnumerable<string> rics) {
                _container = container;
                _setup = setup;
                _rics = rics;
                _res = container.GetInstance<IHistoryContainer>();
                _subscriptions = new Dictionary<string, IHistoryRequest>();
                Logger = logger;
            }

            protected override void Prepare() {
                _loader = _container.GetInstance<IHistory>()
                    .WithFeed(_setup.Feed)
                    .WithNumRecords(_setup.Rows) // todo check for nulls or wut?
                    .WithSince(_setup.Since)     // todo clone?
                    .WithTill(_setup.Till)
                    .WithHistory(container => {
                        this.Trace("Got data()");
                        _res = _res.Import(container);
                        // todo remove appropriate rics from the list of rics awaited
                        // todo when all rics are loaded, mark test as Successfully Finished
                    });
                
                foreach (var ric in _rics) 
                    _subscriptions[ric] = _loader.Subscribe(ric);
            }


            protected override void Perform() {
                Parallel.ForEach(_rics, ric => {
                    _subscriptions[ric].Request();
                });
            }

            protected override void Success() {
                this.Trace("Success()");
                if (_setup.Callback != null)
                    _setup.Callback(_res);
            }

            public ILogger Logger { get; private set; }
        }

        public MultiHistoryRequest(IContainer container, ILogger logger, HistorySetup setup, IEnumerable<string> rics) {
            _algo = container
                .With(setup)
                .With(rics)
                .GetInstance<MultuHistoryAlgorithm>();
            Logger = logger;
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

        public ILogger Logger { get; private set; }
    }
}
