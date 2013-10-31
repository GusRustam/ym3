using System;
using DataProvider.Objects;
using Interop.TSI6;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using StructureMap;
using Toolbox.Async;

namespace DataProvider.Loaders.History {
    public class TsiHistoryRequest : IHistoryRequest, ISupportsLogging {
        private class TsiAlgorithm : TimeoutCall, ISupportsLogging {
            private readonly IContainer _container;
            private readonly HistorySetup _setup;
            private TsiSession _session;

            public TsiAlgorithm(IContainer container, ILogger logger, HistorySetup setup) {
                _container = container;
                _setup = setup;
                Logger = logger;
            }

            protected override void Prepare() {
                var obj = _container.GetInstance<IEikonObjects>();
                var connect = obj.CreateTsiConnectInfo();
                if (connect == null)
                    throw new InvalidOperationException("tsi6 inaccessible");
                var request = obj.CreateTsiConnectRequest();
                if (request == null)
                    throw new InvalidOperationException("tsi6 inaccessible");
                var session = request.Connect(connect);
                if (!session.IsConnected())
                    throw new InvalidOperationException("tsi6 not connected");
                _session = session;
            }

            protected override void Perform() {
                var requestInfo = _container.GetInstance<TsiReqInfo>();
                // ric and feed
                requestInfo.RicFeed.Name = _setup.Ric;
                requestInfo.RicFeed.Feed = _setup.Feed;

                // interval
                requestInfo.Interval.Type = TsiIntervalType.tsiIntervalDaily;
                requestInfo.Interval.Length = 1;

                // fields
                foreach (var field in _setup.Fields)
                    requestInfo.AppendFact(field.TsiName);

                // mode
                var flags = (int)TsiReqFlags.tsiReqFlagLOAD;
                if (_setup.Since.HasValue) {
                    flags += (int)TsiReqFlags.tsiReqFlagUSESTART;
                    requestInfo.Start.Timezone.TzFlag = TsiTimezoneFlag.tsiTimezoneTZLOCAL;
                    requestInfo.Start.ValidFlags = TsiDateTimeFlag.tsiDtFlagOPEN;
                    requestInfo.Start.DateTime = _setup.Since.Value;
                }
                if (_setup.Till.HasValue) {
                    flags += (int)TsiReqFlags.tsiReqFlagUSEEND;
                    requestInfo.End.Timezone.TzFlag = TsiTimezoneFlag.tsiTimezoneTZLOCAL;
                    requestInfo.End.ValidFlags = TsiDateTimeFlag.tsiDtFlagOPEN;
                    requestInfo.End.DateTime = _setup.Till.Value;
                }
                if (_setup.Rows.HasValue) {
                    flags += (int)TsiReqFlags.tsiReqFlagUSEPOINTS;
                    requestInfo.Points = _setup.Rows.Value;
                }
                requestInfo.Flags = (TsiReqFlags)flags;

                var request = _container.GetInstance<TsiGetDataRequest>();
                request.GetData(_session, requestInfo);
                request.OnLoadData += Loading;
                request.OnLoadEnd += LoadFinished;
            }

            private void LoadFinished(TsiGetDataRequest request) {
                lock (LockObj) {
                    // todo parse and extract - see Lukoil
                    TryChangeState(State.Succeded);
                }
            }

            private void Loading(TsiGetDataRequest request, TsiTable table) {
                lock (LockObj) {
                    
                }
            }

            protected override void Success() {
                if (_setup.Callback != null) 
                    _setup.Callback(); // todo data
            }

            public ILogger Logger { get; private set; }
        }

        private readonly TsiAlgorithm _algo;

        // todo in all other similar classes add ILogger logger as a param - let container resolve it itself
        // todo in some cases I'll be able to get rid of IContainer container itself I guess
        public TsiHistoryRequest(IContainer container, ILogger logger, HistorySetup setup)  {
            _algo = new TsiAlgorithm(container, logger, setup);
            Logger = logger;
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

        public ILogger Logger { get; private set; }
    }
}