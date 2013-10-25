using System;
using System.Collections.Generic;
using DataProvider.DataLoaders.Status;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using ThomsonReuters.Interop.RTX;
using Toolbox;

namespace DataProvider.DataLoaders {
    class AdfinSubscription : ISubscription, ISupportsLogging {
        private readonly AdxRtList _adxRtList;
        private readonly string _feed;
        private readonly string _mode;
        private readonly IEnumerable<string> _rics;
        private readonly IEnumerable<string> _fields;

        // Actions necessary for Subscription
        internal Action OnTimeAction { get; private set; }
        internal Action<string, object, IItemStatus> OnUpdateAction { get; private set; }

        public ISubscription OnTime(Action action) {
            OnTimeAction = action;
            return this;
        }

        public ISubscription OnDataUpdated(Action<string, object, IItemStatus> action) {
            OnUpdateAction = action;
            return this;
        }

        internal AdfinSubscription(ILogger logger, AdxRtList adxRtList, string feed, string mode, IEnumerable<string> rics, IEnumerable<string> fields) {
            Logger = logger;
            _adxRtList = adxRtList;
            _feed = feed;
            _mode = mode;
            _rics = rics;
            _fields = fields;

            _adxRtList.OnStatusChange += OnStatusChangeHandler;
            
            _adxRtList.ErrorMode = AdxErrorMode.EXCEPTION;
            _adxRtList.DebugLevel = RT_DebugLevel.RT_DEBUG_IMMEDIATE;
        }

        public void Snapshot() {
            ResetAdxRtList();

            _adxRtList.StartUpdates(RT_RunMode.RT_MODE_IMAGE);
        }

        public void Start(IRunMode mode) {
            ResetAdxRtList();

            if (mode.Equals(RunMode.OnTime) && OnTimeAction != null)
                 _adxRtList.OnTime += OnTimeHandler;
            else if ((mode.Equals(RunMode.OnTimeIfUpdated) || mode.Equals(RunMode.OnUpdate)) && OnUpdateAction != null)
                _adxRtList.OnUpdate += OnUpdateHandler;

            var rtRunMode = mode.ToAdxMode();
            _adxRtList.StartUpdates(rtRunMode);

            // todo do I get it right, that no need to start a separate thread here?
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

        private void ResetAdxRtList() {
            if (OnUpdateAction != null)
                _adxRtList.OnUpdate -= OnUpdateHandler;
            if (OnTimeAction != null)
                _adxRtList.OnTime -= OnTimeHandler;

            if (_adxRtList.ListStatus == RT_ListStatus.RT_LIST_RUNNING) 
                _adxRtList.StopUpdates();
            if (_adxRtList.ListStatus == RT_ListStatus.RT_LIST_UPDATES_STOPPED) 
                _adxRtList.UnregisterAllItems();
            
            _adxRtList.Source = _feed;
            _adxRtList.Mode = _mode;

            _adxRtList.RegisterItems(String.Join(",", _rics), String.Join(",", _fields));
        }

        private void OnUpdateHandler(string name, object tag, RT_ItemStatus status) {
            // todo! I have already did OnUpdate - see Fields
            if (OnUpdateAction != null)
                OnUpdateAction(name, tag, ItemStatus.FromAdxStatus(status));
        }

        private void OnTimeHandler() {
            // todo! Similar to OnImage - see Snapshot, I guess
            if (OnTimeAction != null)
                OnTimeAction();
        }

        private void OnStatusChangeHandler(RT_ListStatus status, RT_SourceStatus sourceStatus, RT_RunMode mode) {
            // todo notify user on source failures
            if (OnFeedStatusAction != null) OnFeedStatusAction();
        }

        public ILogger Logger { get; set; }
    }
}