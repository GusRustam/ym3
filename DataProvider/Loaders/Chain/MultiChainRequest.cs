using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using StructureMap;
using Toolbox.Async;

namespace DataProvider.Loaders.Chain {
    public class MultiChainRequest : IChainRequest, ISupportsLogging {
        private readonly MultiChainAlgo _algo;

        public class MultiChainAlgo : TimeoutCall, ISupportsLogging {
            private readonly IContainer _container;
            private readonly ChainSetup _setup;
            private readonly IChainData _res;
            private readonly IList<IChainRequest> _requests;
            private readonly IList<string> _chainRics;

            public MultiChainAlgo(IContainer container, ILogger logger, IChainData res, ChainSetup setup) {
                Logger = logger;
                _container = container;
                _res = res;
                _setup = setup;
                _chainRics = new List<string>(setup.Rics);
                _requests = new List<IChainRequest>();
            }

            protected override void Prepare() {
                this.Trace("Prepare()");
                var loader = _container
                    .GetInstance<IChain>();

                if (!string.IsNullOrEmpty(_setup.Mode)) 
                    loader = loader.WithMode(_setup.Mode);

                loader = loader.WithChain(data => {
                    lock (LockObj) {
                        this.Trace(string.Format("WithChain(rics: {0})", data.Data.Keys.Count));
                        try {
                            foreach (var item in data.Data) {
                                this.Trace(string.Format("Joining chains! Got ric {0}", item.Key));

                                _res.Data.Add(item.Key, new List<string>(item.Value));
                                _chainRics.Remove(item.Key);
                            }
                            if (!_chainRics.Any())
                                TryChangeState(State.Succeded);
                        } catch (Exception e) {
                            Report = e;
                            TryChangeState(State.Invalid);
                        }
                    }
                })
                .WithFeed(_setup.Feed);

                foreach (var ric in _setup.Rics) 
                    _requests.Add(loader
                        .WithRics(ric)
                        .Subscribe());
            }

            protected override void Perform() {
                this.Trace("Perform()");
                Parallel.ForEach(_requests, request => 
                    request
                        .WithErrorCallback(exception => {
                            lock (LockObj) {
                                foreach (var ric in request.Rics) 
                                    _chainRics.Remove(ric);
                                if (!_chainRics.Any())
                                    TryChangeState(State.Succeded);
                            }
                        })
                        .Request());
            }

            protected override void Success() {
                this.Trace("Success()");
                if (_setup.Callback != null)
                    _setup.Callback(_res);
            }

            public ILogger Logger { get; private set; }
        }

        public MultiChainRequest(IContainer container, ILogger logger, ChainSetup setup) {
            Rics = new List<string>(setup.Rics);
            Logger = logger;
            _algo = container
                .With(setup)
                .GetInstance<MultiChainAlgo>();
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
        public IList<string> Rics { get; private set; }
    }
}