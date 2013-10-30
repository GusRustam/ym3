using System;
using System.Collections.Generic;
using System.Linq;
using DataProvider.Loaders.Realtime.Data;
using DataProvider.Loaders.Status;
using LoggingFacility.LoggingSupport;
using StructureMap;
using ThomsonReuters.Interop.RTX;
using Toolbox;

namespace DataProvider.Loaders.Realtime {
    public class AdfinFieldsTicker : AdfinTimeoutRequest, IFieldsTicker {
        private int _countdown;

        // Callbacks
        private Action<IRicsFields> _callback;

        // Adfin status
        private ISourceStatus _status = SourceStatus.Unknown;

        // Storage for return data
        private readonly Dictionary<string, IRicFields> _fields = new Dictionary<string, IRicFields>();

        // todo how to initialize base class using IContainer
        public AdfinFieldsTicker(IContainer container, TimeSpan? timeout, string[] rics, string feed) :
            base(container, timeout, rics, feed) {
        }

        /// <summary>
        /// Use specific data callback
        /// </summary>
        /// <param name="callback">WithCallback</param>
        public IFieldsTicker WithFields(Action<IRicsFields> callback) {
            _callback = callback;
            return this;
        }

        private void OnUpdateHandler(string ric, object tag, RT_ItemStatus status) {
            lock (LockObj) { // too much inside operations to allow it work in async mode
                // informing
                this.Trace(string.Format("OnUpdateHandler({0}, {1})", ric, status));
                this.Trace(string.Format("Entering: {0} items left to load, registered {1} items", _countdown,
                    AdxRtList.ItemListCount[RT_ItemRowView.RT_IRV_ALL]));

                // sanity check
                if (_fields.ContainsKey(ric)) {
                    this.Warn(string.Format("Ric {0} fields already loaded", ric));
                    return;
                }

                var itemStatus = ItemStatus.FromAdxStatus(status);
                if (status.Belongs(RT_ItemStatus.RT_ITEM_DELAYED, RT_ItemStatus.RT_ITEM_OK)
                    || !Rics.Contains(ric)) { // if ric is valid, store it


                    // get list of fields
                    var data = (object[,])AdxRtList.ListFields[
                        ric,
                        RT_FieldRowView.RT_FRV_UPDATED,
                        RT_FieldColumnView.RT_FCV_STATUS];

                    // allocate
                    var length = data.GetLength(0);
                    var currentFields = new string[length];

                    // store
                    for (var i = 0; i < length; i++)
                        currentFields[i] = data[i, 0].ToString();

                    // save
                    _fields[ric] = new RicFields(itemStatus, currentFields);

                } else { // ric is invalid, indicate it
                    _fields[ric] = new RicFields(itemStatus);
                }

                // no need to have any more updates on this ric
                AdxRtList.UnregisterItems(ric);

                // done?
                if (--_countdown == 0) {
                    this.Trace("Countdown finished, bye");
                    TryChangeState(State.Succeded);
                    //CancelSrc.Cancel();
                }

                // moar info
                this.Trace(string.Format("Exiting: {0} items left to load, registered {1} items", _countdown,
                    AdxRtList.ItemListCount[RT_ItemRowView.RT_IRV_ALL]));
            }
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

        protected override void RegisterHandlers() {
            _countdown = Rics.Count();
            AdxRtList.OnStatusChange += OnStatusChangeHandler;
            AdxRtList.OnUpdate += OnUpdateHandler;
            AdxRtList.RegisterItems(String.Join(",", Rics), "*");
        }

        protected override void ReportInvalid() {
            if (_callback != null)
                _callback(new RicsFields(_status));
        }

        protected override void ReportOk() {
            if (_callback != null)
                _callback(new RicsFields(_status, _fields));
        }

        protected override IRunMode AdfinMode {
            get { return RunMode.OnUpdate; }
        }
    }
}