using System;
using System.Collections.Generic;
using System.Linq;
using DataProvider.Objects;
using LoggingFacility;
using LoggingFacility.LoggingSupport;

namespace DataProvider.Loaders.Realtime {
    class AdfinSubscriptionSetup : ISubscriptionSetup, ISupportsLogging {
        private readonly IEikonObjects _objects;
        private readonly IEnumerable<string> _rics;
        private readonly string _feed;

        private string _mode;
        private IEnumerable<string> _fields;

        // Constructor
        public AdfinSubscriptionSetup(IEnumerable<string> rics, string feed, IEikonObjects objects, ILogger logger) {
            _rics = rics;
            _feed = feed;
            _objects = objects;
            Logger = logger;
        }

        // Flow-style setters
        public ISubscriptionSetup WithFrq(TimeSpan span) {
            if (span > TimeSpan.FromDays(1)) 
                throw new InvalidOperationException("refresh period too long");
            
            if (span < TimeSpan.FromSeconds(1))
                throw new InvalidOperationException("refresh period too short");

            var addon = "";
            if (span > TimeSpan.FromHours(1))
                addon = string.Format("FRQ:{0}H", span.Hours);
            else if (span > TimeSpan.FromMinutes(1))
                addon = string.Format("FRQ:{0}M", span.Minutes);
            else
                addon = string.Format("FRQ:{0}S", span.Seconds);

            _mode = string.IsNullOrEmpty(_mode) ? addon : string.Format("{0} {1}", _mode, addon);
            
            return this;
        }

        public ISubscriptionSetup WithMode(string mode) {
            _mode = string.IsNullOrEmpty(_mode) ? mode : string.Format("{0} {1}", _mode, mode);
            return this;
        }

        public ISubscriptionSetup WithFields(params string[] fields) {
            if (fields == null || !fields.Any())
                throw new InvalidOperationException("rics");

            _fields = (_fields == null) ? fields : _fields.Concat(fields);
            return this;
        }

        public ISnapshotTicker SnapshotReuqest(TimeSpan? timeout = null) {
            Validate(true);
            return new AdfinSnapshotTicker(
                Logger, _objects.CreateAdxRtList(), timeout, 
                _rics.ToArray(), _feed,  _fields.ToArray());
        }

        public IFieldsTicker FieldsRequest(TimeSpan? timeout = null) {
            Validate(false);
            return new AdfinFieldsTicker(
                Logger, _objects.CreateAdxRtList(), timeout, 
                _rics.ToArray(), _feed);
        }

        // Entrance to the next level
        public ISubscription DataRequest() {
            if (string.IsNullOrEmpty(_feed)) throw new InvalidOperationException("Invalid feed");
            Validate(true);
            return new AdfinSubscription(
                Logger, _objects.CreateAdxRtList(), 
                _feed, _mode, _rics, _fields);
        }

        private void Validate(bool checkFields) {
            if (_rics == null || !_rics.Any())
                throw new InvalidOperationException("No rics");
            if (checkFields && (_fields == null || !_fields.Any()))
                throw new InvalidOperationException("No fields");
            if (_feed == "")
                throw new InvalidOperationException("feed");
        }

        public ILogger Logger { get; private set; }
    }
}