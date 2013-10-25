using System;
using DataProvider.DataLoaders.Status;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using ThomsonReuters.Interop.RTX;

namespace DataProvider.DataLoaders {
    internal class AdfinSnapshotTicker : AdfinTimeoutRequest, ISnapshotTicker {
        private Action<IDataStatus> _callbackAction;

        // Adfin status
        private ISourceStatus _status = SourceStatus.Unknown;
        private readonly string[] _fields;

        protected override void RegisterHandlers() {
            AdxRtList.OnStatusChange += OnStatusChangeHandler;
            AdxRtList.OnImage += OnImageHandler;
            AdxRtList.RegisterItems(String.Join(",", Rics), string.Join(",", _fields));
        }

        protected override void ReportInvalid() {
            throw new NotImplementedException();
        }

        protected override void ReportOk() {
            throw new NotImplementedException();
        }

        protected override IRunMode AdfinMode {
            get { return RunMode.Snapshot; }
        }

        public ISnapshotTicker WithCallback(Action<IDataStatus> onImage) {
            _callbackAction = onImage;
            return this;
        }

        public AdfinSnapshotTicker(ILogger logger, AdxRtList adxRtList, TimeSpan? timeout, string[] rics, string feed, string[] fields) : 
            base(logger, adxRtList, timeout, rics, feed) {
            _fields = fields;
        }

        private void OnImageHandler(RT_DataStatus aDatastatus) {

        }

        private void OnStatusChangeHandler(RT_ListStatus aListstatus, RT_SourceStatus aSourcestatus, RT_RunMode aRunmode) {
            lock (LockObj) {
                this.Trace(string.Format("OnStatusChangeHandler({0}, {1}, {2})", aListstatus, aSourcestatus, aRunmode));

                // save status
                _status = SourceStatus.FromAdxStatus(aSourcestatus);

                if (aSourcestatus == RT_SourceStatus.RT_SOURCE_UP)
                    return;

                // unregister all requests
                if (AdxRtList.ListStatus == RT_ListStatus.RT_LIST_RUNNING)
                    AdxRtList.UnregisterAllItems();

                // time to stop stupid attempts, source not up.
                TryChangeState(State.Invalid); // indicate failure
            }
        }

    }
}