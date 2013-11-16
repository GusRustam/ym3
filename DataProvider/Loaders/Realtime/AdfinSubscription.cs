using System;
using System.Collections.Generic;
using DataProvider.Loaders.Realtime.Data;
using DataProvider.Loaders.Status;
using DataProvider.Objects;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using ThomsonReuters.Interop.RTX;
using Toolbox;

namespace DataProvider.Loaders.Realtime {
    public class AdfinSubscription : ISubscription, ISupportsLogging {
        private readonly AdxRtList _adxRtList;
        private readonly string _feed;
        private readonly string _mode;
        private readonly IEnumerable<string> _rics;
        private readonly IEnumerable<string> _fields;

        // Actions necessary for Subscription
        private Action<ISnapshot> _callback;
        private Action<string, ISourceStatus, IListStatus> _onSourceAction;

        // Return values
        private ISourceStatus _status;
        private IListStatus _listStatus;

        // Subscription
        public ISubscription WithStatus(Action<string, ISourceStatus, IListStatus> action) {
            _onSourceAction = action;
            return this;
        }

        public ISubscription WithCallback(Action<ISnapshot> action) {
            _callback = action;
            return this;
        }

        // constructor
        public AdfinSubscription(string feed, string mode, IEnumerable<string> rics, IEnumerable<string> fields, ILogger logger, IEikonObjects objects) {
            Logger = logger;
            _adxRtList = objects.CreateAdxRtList();
            _feed = feed;
            _mode = mode;
            _rics = rics;
            _fields = fields;

            _adxRtList.OnStatusChange += OnStatusChangeHandler;
            
            _adxRtList.ErrorMode = AdxErrorMode.EXCEPTION;
            _adxRtList.DebugLevel = RT_DebugLevel.RT_DEBUG_IMMEDIATE;
        }

        public void Start(IRunMode mode) {
            ResetAdxRtList();

            if (mode.Equals(RunMode.OnTime) && _callback != null)
                 _adxRtList.OnTime += OnTimeHandler;
            else if ((mode.Equals(RunMode.OnTimeIfUpdated) || mode.Equals(RunMode.OnUpdate)) && _callback != null)
                _adxRtList.OnUpdate += OnUpdateHandler;

            var rtRunMode = mode.ToAdxMode();
            _adxRtList.StartUpdates(rtRunMode);
    }

        public void Stop() {
            if (_adxRtList.ListStatus == RT_ListStatus.RT_LIST_RUNNING) 
                _adxRtList.StopUpdates();
        }

        public void Close() {
            if (!_adxRtList.ListStatus.Belongs(RT_ListStatus.RT_LIST_RUNNING, RT_ListStatus.RT_LIST_UPDATES_STOPPED)) return;
            
            _adxRtList.UnregisterAllItems();
            _adxRtList.CloseAllLinks();
        }

        // private methods
        private void ResetAdxRtList() {
            _adxRtList.OnUpdate -= OnUpdateHandler;
            _adxRtList.OnTime -= OnTimeHandler;

            if (_adxRtList.ListStatus == RT_ListStatus.RT_LIST_RUNNING) 
                _adxRtList.StopUpdates();
            if (_adxRtList.ListStatus == RT_ListStatus.RT_LIST_UPDATES_STOPPED) 
                _adxRtList.UnregisterAllItems();
            
            _adxRtList.Source = _feed;
            _adxRtList.Mode = _mode;

            _adxRtList.RegisterItems(String.Join(",", _rics), String.Join(",", _fields));
        }

        private void OnUpdateHandler(string ric, object tag, RT_ItemStatus status) {
            this.Trace(string.Format("OnUpdateHandler({0}, {1})", ric, status));

            var snapshot = new List<ISnapshotItem>();

            var ricStatus = ItemStatus.FromAdxStatus(status);
            var item = new SnapshotItem(ric, ricStatus);

            this.Trace(string.Format("Got ric {0} state {1}", ric, ricStatus));
            
            var fieldStatuses = (object[,])_adxRtList.ListFields[
                    ric, RT_FieldRowView.RT_FRV_UPDATED, RT_FieldColumnView.RT_FCV_STATUS];
            
            var fields = (object[,])_adxRtList.ListFields[
                    ric, RT_FieldRowView.RT_FRV_UPDATED, RT_FieldColumnView.RT_FCV_VALUE];

            for (var j = 0; j < fields.GetLength(0); j++) {
                var fieldName = fields.GetValue(j, 0).ToString();
                var fieldValue = fields.GetValue(j, 1).ToString();
                var fieldStatus = FieldStatus.FromAdxStatus((RT_FieldStatus)fieldStatuses.GetValue(j, 1));

                item.AddField(fieldName, fieldValue, fieldStatus);

                this.Trace(string.Format(" -> field {0} value {1} status {2}",
                    fieldName, fieldValue,
                    fieldStatus));
            }
            snapshot.Add(item);
            
            if (_callback != null)
                _callback(new Snapshot(_status, _listStatus, snapshot));
        }

        private void OnTimeHandler() {
            var rics = (object[,])_adxRtList.ListItems[RT_ItemRowView.RT_IRV_ALL, RT_ItemColumnView.RT_ICV_STATUS];
            var snapshot = new List<ISnapshotItem>();
            for (var i = 0; i < rics.GetLength(0); i++) {
                var ric = rics[i, 0].ToString();
                var ricStatus = ItemStatus.FromAdxStatus((RT_ItemStatus)rics[i, 1]);

                var item = new SnapshotItem(ric, ricStatus);

                this.Trace(string.Format("Got ric {0} state {1}", ric, ricStatus));

                // if use RT_FieldRowView.RT_FRV_UPDATED then it is equivalent to mode OnTimeIfUpdated
                var fieldStatuses = (object[,])_adxRtList.ListFields[
                        ric, RT_FieldRowView.RT_FRV_ALL, RT_FieldColumnView.RT_FCV_STATUS]; 

                var fields = (object[,])_adxRtList.ListFields[
                        ric, RT_FieldRowView.RT_FRV_ALL, RT_FieldColumnView.RT_FCV_VALUE];

                for (var j = 0; j < fields.GetLength(0); j++) {
                    var fieldName = fields.GetValue(j, 0).ToString();
                    var fieldValue = fields.GetValue(j, 1).ToString();
                    var fieldStatus = FieldStatus.FromAdxStatus((RT_FieldStatus)fieldStatuses.GetValue(j, 1));

                    item.AddField(fieldName, fieldValue, fieldStatus);

                    this.Trace(string.Format(" -> field {0} value {1} status {2}",
                        fieldName, fieldValue,
                        fieldStatus));
                }
                snapshot.Add(item);
            }
            if (_callback != null)
                _callback(new Snapshot(_status, _listStatus ,snapshot));
        }

        private void OnStatusChangeHandler(RT_ListStatus status, RT_SourceStatus sourceStatus, RT_RunMode mode) {
            this.Trace(string.Format("OnStatusChangeHandler({0}, {1}, {2})", ListStatus.FromAdxStatus(status), SourceStatus.FromAdxStatus(sourceStatus), RunMode.FromAdxStatus(mode)));
            _status = SourceStatus.FromAdxStatus(sourceStatus);
            _listStatus = ListStatus.FromAdxStatus(status);
            if (_onSourceAction != null)
                _onSourceAction(_feed, _status, _listStatus);
        }

        public ILogger Logger { get; set; }
    }
}

//------------------------------------------------------------------------------------------------------------------
// Internet connection fail scenario 1 (timeout = 120 seconds, Eikon has enough time to observe feeds are down)
//------------------------------------------------------------------------------------------------------------------
//
// -- regular update
// 10/29/2013 10:25:18 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	 OnUpdateHandler(EUR=, RT_ITEM_OK)
// 10/29/2013 10:25:18 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	 Got ric EUR= state Ok
// 10/29/2013 10:25:18 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	  -> field ASK value +1.3789 status Ok
// 10/29/2013 10:25:18 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	  -> field BID value +1.3786 status Ok
// Update with source status Up and list status Running
// Got update on ric EUR=
//
// -- Feed staus -> down after TWO minutes of waiting
// 10/29/2013 10:27:15 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	 OnStatusChangeHandler(Running, Down, OnUpdate)
// Feed IDN -> feed status Down, list status Running
//
// -- Some updates with stale item status
// 10/29/2013 10:27:15 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	 OnUpdateHandler(EUR=, RT_ITEM_STALE)
// 10/29/2013 10:27:15 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	 Got ric EUR= state UnknownOrInvalid
// 10/29/2013 10:27:15 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	  -> field ASK value +1.3789 status UnknownOrUndefined
// 10/29/2013 10:27:15 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	  -> field BID value +1.3786 status UnknownOrUndefined
// Update with source status Down and list status Running
// Got update on ric EUR=
//
// 10/29/2013 10:27:15 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	 OnUpdateHandler(GAZP.MM, RT_ITEM_STALE)
// 10/29/2013 10:27:15 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	 Got ric GAZP.MM state UnknownOrInvalid
// 10/29/2013 10:27:15 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	  -> field BID value +150.62 status UnknownOrUndefined
// 10/29/2013 10:27:15 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	  -> field ASK value +150.68 status UnknownOrUndefined
// Update with source status Down and list status Running
// Got update on ric GAZP.MM
//
// 10/29/2013 10:27:16 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	 OnStatusChangeHandler(Inactive, Down, Unknown)
// Feed IDN -> feed status Down, list status Inactive
//
// 10/29/2013 10:27:16 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	 OnStatusChangeHandler(Inactive, Down, Unknown)
// Feed IDN -> feed status Down, list status Inactive
//
// 10/29/2013 10:27:16 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	 OnStatusChangeHandler(Inactive, Down, Unknown)
// Feed IDN -> feed status Down, list status Inactive


//------------------------------------------------------------------------------------------------------------------
// Internet connection fail scenario 2 (timeout = 60 seconds, Eikon didn't have enough time to observe feeds are down)
//------------------------------------------------------------------------------------------------------------------
//
// -- Last regular update
//
// 10/29/2013 10:33:25 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	 OnUpdateHandler(EUR=, RT_ITEM_OK)
// 10/29/2013 10:33:25 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	 Got ric EUR= state Ok
// 10/29/2013 10:33:25 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	  -> field ASK value +1.3784 status Ok
// 10/29/2013 10:33:25 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	  -> field BID value +1.3783 status Ok
// Update with source status Up and list status Running
// Got update on ric EUR=
//
// -- Now feed is still UP, but the list in Inactive (ONE minute of waiting)
// 10/29/2013 10:34:24 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	 OnStatusChangeHandler(Inactive, Up, Unknown)
// Feed IDN -> feed status Up, list status Inactive
// 10/29/2013 10:34:24 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	 OnStatusChangeHandler(Inactive, Up, Unknown)
// Feed IDN -> feed status Up, list status Inactive
// 10/29/2013 10:34:24 	 Trace 	 DataProvider.RawData.EikonObjectsSdk 	 OnStatusChangeHandler(Inactive, Up, Unknown)
// Feed IDN -> feed status Up, list status Inactive

//------------------------------------------------------------------------------------------------------------------
// Conclusion
//------------------------------------------------------------------------------------------------------------------
//
// I have to catch both ListStatus -> Inactive and FeedStatus -> Down when watching the list. I could also join them 
// into single logical state but I am currently unsure how to do that
//
//