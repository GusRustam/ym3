using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataProvider.Annotations;
using DataProvider.Loaders.History.Data;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using StructureMap;
using Toolbox.Async;

namespace DataProvider.Loaders.History {
    public class MultiHistoryRequest : IHistoryRequest, ISupportsLogging  {
        private readonly MultiHistoryAlgorithm _algo;

        [UsedImplicitly]
        private class MultiHistoryAlgorithm : TimeoutCall, ISupportsLogging {
            private readonly IContainer _container;
            private readonly HistorySetup _setup;
            private readonly IList<string> _rics;
            private readonly ISet<string> _ricsToLoad;
            private readonly IDictionary<string, IHistoryRequest> _subscriptions;

            private IHistory _loader;
            private IHistoryContainer _res;
            private Action<IHistoryContainer> _originalCallback;

            public MultiHistoryAlgorithm(IContainer container, IHistoryContainer res, ILogger logger, HistorySetup setup, string[] rics) {
                _container = container;
                _setup = setup;
                _rics = rics.ToList();
                _ricsToLoad = new HashSet<string>(_rics);
                _res = res;
                _subscriptions = new Dictionary<string, IHistoryRequest>();
                Logger = logger;
            }

            protected override void Prepare() {
                this.Trace("Prepare()");
                _loader = _container
                    .GetInstance<IHistory>()
                    .AppendFields(_setup.Fields)
                    .WithFeed(_setup.Feed);

                if (_setup.Rows.HasValue)
                    _loader = _loader.WithNumRecords(_setup.Rows.Value);

                if (_setup.Since.HasValue)
                    _loader = _loader.WithSince(_setup.Since.Value);

                if (_setup.Till.HasValue)
                    _loader = _loader.WithTill(_setup.Till.Value);

                _originalCallback = _setup.Callback;

                _loader = _loader
                    .WithHistory(container => {
                        this.Trace(string.Format("Callback on History, rics = [{0}]", string.Join(",",container.Slice1()[0])));

                        // importing data
                        _res = _res.Import(container);
                    
                        // removing rics from list of rics to be loaded
                        foreach (var ric in container.Slice1())  _ricsToLoad.Remove(ric);

                        // if all rics are loaded, notify that we've finished successfully
                        if (!_ricsToLoad.Any()) TryChangeState(State.Succeded);
                    });
                
                this.Trace(string.Format("Rics are {0}", string.Join(", ", _rics)));
                foreach (var ric in _rics)
                    // this call creates new object, and hence 
                    // Request call can be performed in parallel
                    _subscriptions[ric] = _loader.Subscribe(ric);
            }

            protected override void Perform() {
                this.Trace("Perform()");
                //foreach (var ric in _rics) _subscriptions[ric].Request();
                Parallel.ForEach(_rics, ric => _subscriptions[ric]
                    //.WithErrorCallback(exception => {
                    //    _ricsToLoad.Remove(ric);
                    //    if (!_ricsToLoad.Any()) TryChangeState(State.Succeded);
                    //})
                    .Request());
            }

            protected override void Finish() {
                this.Trace("Success()");
                if (_originalCallback != null)
                    _originalCallback(_res);
            }

            protected override void HandleTimout() {
                lock (LockObj) {
                    _res.Status = TimeoutStatus.Timeout;
                    FillStatuses();
                }
            }

            protected override void HandleError(Exception ex) {
                lock (LockObj) {
                    _res.Status = TimeoutStatus.CreateError(ex);
                    FillStatuses();
                }
            }

            protected override void HandleCancel() {
                lock (LockObj) {
                    _res.Status = TimeoutStatus.Cancelled;
                    FillStatuses();
                }
            }

            private void FillStatuses() {
                lock (LockObj) {
                    foreach (var ric in _ricsToLoad) {
                        _res.RicStatuses[ric] = _res.Status;
                        _subscriptions[ric].Cancel();
                    }
                    _ricsToLoad.Clear();
                }
            }


            public ILogger Logger { get; private set; }
        }

        public MultiHistoryRequest(IContainer container, ILogger logger, HistorySetup setup, string[] rics) {
            Logger = logger;
            this.Trace(string.Format("Rics are {0}", string.Join(", ", rics)));
            _algo = container
                .With(setup)
                .With(typeof(string[]), rics)
                .GetInstance<MultiHistoryAlgorithm>();
        }

        ////  down here - wut?
        //public ITimeoutCall WithCancelCallback(Action callback) {
        //    _algo.WithCancelCallback(callback);
        //    return this;
        //}

        //public ITimeoutCall WithTimeoutCallback(Action callback) {
        //    _algo.WithTimeoutCallback(callback);
        //    return this;
        //}

        //public ITimeoutCall WithErrorCallback(Action<Exception> callback) {
        //    _algo.WithErrorCallback(callback);
        //    return this;
        //}

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
