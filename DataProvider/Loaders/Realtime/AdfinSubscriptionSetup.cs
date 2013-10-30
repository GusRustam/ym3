using System;
using System.Collections.Generic;
using System.Linq;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using StructureMap;

namespace DataProvider.Loaders.Realtime {
    public class AdfinSubscriptionSetup : ISubscriptionSetup, ISupportsLogging {
        private readonly IEnumerable<string> _rics;
        private readonly string _feed;
        private readonly IContainer _container;

        private string _mode;
        private IEnumerable<string> _fields;

        // Constructor
        public AdfinSubscriptionSetup(IEnumerable<string> rics, string feed, IContainer container) {
            _rics = rics;
            _feed = feed;
            _container = container;
            Logger = container.GetInstance<ILogger>();
        }

        // Flow-style setters
        public ISubscriptionSetup WithFrq(TimeSpan span) {
            if (span > TimeSpan.FromDays(1)) 
                throw new InvalidOperationException("refresh period too long");
            
            if (span < TimeSpan.FromSeconds(1))
                throw new InvalidOperationException("refresh period too short");

            string addon;
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
            return _container
                .With("rics").EqualTo(_rics.ToArray())
                .With("feed").EqualTo(_feed)
                .With("timeout").EqualTo(timeout)
                .With("fields").EqualTo(_fields.ToArray())
                .GetInstance<ISnapshotTicker>();
        }

        public IFieldsTicker FieldsRequest(TimeSpan? timeout = null) {
            Validate(false);
            return _container
               .With("rics").EqualTo(_rics.ToArray())
               .With("timeout").EqualTo(timeout)
               .With("feed").EqualTo(_feed)
               .GetInstance<IFieldsTicker>();
        }

        // Entrance to the next level
        public ISubscription DataRequest() {
            if (string.IsNullOrEmpty(_feed)) throw new InvalidOperationException("Invalid feed");
            Validate(true);
            return _container
               .With("rics").EqualTo(_rics)
               .With("fields").EqualTo(_fields)
               .With("mode").EqualTo(_mode)
               .With("feed").EqualTo(_feed)
               //.With("containter").EqualTo(_container)
               .GetInstance<ISubscription>();
            //return new AdfinSubscription(
            //    Logger, _objects.CreateAdxRtList(), 
            //    _feed, _mode, _rics, _fields);
        }

// ReSharper disable UnusedParameter.Local
        private void Validate(bool checkFields) {
// ReSharper restore UnusedParameter.Local
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