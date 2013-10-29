using System;
using System.Collections.Generic;
using DataProvider.Loaders.Realtime.Data;
using DataProvider.Loaders.Status;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using ThomsonReuters.Interop.RTX;

namespace DataProvider.Loaders.Realtime {
    internal class AdfinSnapshotTicker : AdfinTimeoutRequest, ISnapshotTicker {
        private Action<ISnapshot> _callbackAction;

        // Adfin status
        private readonly string[] _fields;

        // Return data
        private ISourceStatus _status;
        private IListStatus _listStatus;
        private readonly List<ISnapshotItem> _snapshot;

        protected override void RegisterHandlers() {
            AdxRtList.OnStatusChange += OnStatusChangeHandler;
            AdxRtList.OnImage += OnImageHandler;
            AdxRtList.RegisterItems(String.Join(",", Rics), string.Join(",", _fields));
        }

        protected override void ReportInvalid() {
            if (_callbackAction != null) 
                _callbackAction(new Snapshot(_status, _listStatus));
        }

        protected override void ReportOk() {
            if (_callbackAction != null)
                _callbackAction(new Snapshot(_status, _listStatus, _snapshot));
        }

        protected override IRunMode AdfinMode {
            get { return RunMode.Snapshot; }
        }

        public ISnapshotTicker WithCallback(Action<ISnapshot> onImage) {
            _callbackAction = onImage;
            return this;
        }

        public AdfinSnapshotTicker(ILogger logger, AdxRtList adxRtList, TimeSpan? timeout, string[] rics, string feed, string[] fields) : 
            base(logger, adxRtList, timeout, rics, feed) {
            _fields = fields;
            _snapshot = new List<ISnapshotItem>();
        }

        private void OnImageHandler(RT_DataStatus aDatastatus) {
            lock (LockObj) {
                switch (aDatastatus) {
                    case RT_DataStatus.RT_DS_FULL: // load data
                        var rics = (object[,]) AdxRtList.ListItems[RT_ItemRowView.RT_IRV_ALL, RT_ItemColumnView.RT_ICV_STATUS];
                        for (var i = 0; i < rics.GetLength(0); i++) {
                            var ric = rics[i, 0].ToString();
                            var ricStatus = ItemStatus.FromAdxStatus((RT_ItemStatus) rics[i, 1]);

                            var item = new SnapshotItem(ric, ricStatus);
                            
                            this.Trace(string.Format("Got ric {0} state {1}", ric, ricStatus));
                            
                            var fieldStatuses = (object[,])AdxRtList.ListFields[
                                    ric, RT_FieldRowView.RT_FRV_UPDATED, RT_FieldColumnView.RT_FCV_STATUS];

                            var fields = (object[,])AdxRtList.ListFields[
                                    ric, RT_FieldRowView.RT_FRV_UPDATED, RT_FieldColumnView.RT_FCV_VALUE];


                            for (var j = 0; j < fields.GetLength(0); j++) {
                                var fieldName = fields.GetValue(j, 0).ToString();
                                var fieldValue = fields.GetValue(j, 1).ToString();
                                var fieldStatus = FieldStatus.FromAdxStatus((RT_FieldStatus) fieldStatuses.GetValue(j, 1));

                                item.AddField(fieldName, fieldValue, fieldStatus);

                                this.Trace(string.Format(" -> field {0} value {1} status {2}",
                                    fieldName, fieldValue, 
                                    fieldStatus));
                            }
                            _snapshot.Add(item);
                        }
                        TryChangeState(State.Succeded);
                        break;

                    case RT_DataStatus.RT_DS_PARTIAL: // tell the boss
                        this.Trace("State partial. What should that mean?");
                        break;

                    default: // report error
                        TryChangeState(State.Invalid); // indicate failure
                        break;
                }
            }
        }

        private void OnStatusChangeHandler(RT_ListStatus aListstatus, RT_SourceStatus aSourcestatus, RT_RunMode aRunmode) {
            lock (LockObj) {
                this.Trace(string.Format("OnStatusChangeHandler({0}, {1}, {2})", aListstatus, aSourcestatus, aRunmode));

                _listStatus = ListStatus.FromAdxStatus(aListstatus);

                // save status
                _status = SourceStatus.FromAdxStatus(aSourcestatus);

                if (aSourcestatus == RT_SourceStatus.RT_SOURCE_UP)
                    return;

                // source not up! unregister all requests
                if (AdxRtList.ListStatus == RT_ListStatus.RT_LIST_RUNNING)
                    AdxRtList.UnregisterAllItems();

                // time to stop stupid attempts, source not up.
                TryChangeState(State.Invalid); // indicate failure
            }
        }
    }
}