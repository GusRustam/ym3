using System;
using System.Collections.Generic;
using System.Linq;
using DataProvider.RawData;
using LoggingFacility;
using LoggingFacility.LoggingSupport;

namespace DataProvider.DataLoaders {
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
            
            if (span > TimeSpan.FromHours(1)) 
                _mode = string.Format("FRQ:{0}H", span.Hours);
            else if (span > TimeSpan.FromMinutes(1))
                _mode = string.Format("FRQ:{0}M", span.Minutes);
            else
                _mode = string.Format("FRQ:{0}S", span.Seconds);
            
            return this;
        }

        public ISubscriptionSetup WithFields(params string[] fields) {
            if (fields == null || !fields.Any())
                throw new InvalidOperationException("rics");

            _fields = (_fields == null) ? fields : _fields.Concat(fields);
            return this;
        }

        public ISnapshotTicker ReuqestSnapshot(TimeSpan? timeout = null) {
            //return new AdfinSnapshotTicker(this, Logger, _objects.CreateAdxRtList(), timeout);
            return null;
        }

        public IFieldsTicker RequestFields(TimeSpan? timeout = null) {
            return new AdfinFieldsTicker(Logger, _objects.CreateAdxRtList(), timeout, _rics.ToArray(), _feed);
        }

        // Entrance to the next level
        public ISubscription Create() {
            if (string.IsNullOrEmpty(_feed)) throw new InvalidOperationException("Invalid feed");
            if (_fields == null || !_fields.Any())
                throw new InvalidOperationException("No fields");
            return new AdfinSubscription(Logger, _objects.CreateAdxRtList(), _feed, _mode, _rics, _fields);
        }

        public ILogger Logger { get; private set; }
    }
}