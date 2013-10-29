using System;
using System.Collections.Generic;
using System.Linq;
using DataProvider.Objects;
using LoggingFacility;
using LoggingFacility.LoggingSupport;

namespace DataProvider.Loaders.Realtime {
    public class AdfinRealtime : IRealtime, ISupportsLogging {
        private readonly IEikonObjects _objects;
        private string _feed = "Q";
        private List<string> _rics;

        public AdfinRealtime(IEikonObjects objects, ILogger logger) {
            _objects = objects;
            if (objects == null)
                throw new InvalidOperationException("objects");
            Logger = logger;
        }

        public IRealtime WithFeed(string feed) {
            if (string.IsNullOrEmpty(feed))
                throw new InvalidOperationException("feed");
            _feed = feed;
            return this;
        }

        public IRealtime WithRics(params string[] rics) {
            if (rics == null || !rics.Any())
                throw new InvalidOperationException("rics");
            _rics = new List<string>(rics);
            return this;
        }

        public ISubscriptionSetup Subscribe() {
            if (_rics == null || !_rics.Any())
                throw new InvalidOperationException("rics");
            return new AdfinSubscriptionSetup(_rics, _feed, _objects, Logger);
        }

        public ILogger Logger { get; set; }
    }
}