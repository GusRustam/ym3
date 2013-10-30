using System;
using System.Collections.Generic;
using System.Linq;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using StructureMap;

namespace DataProvider.Loaders.Realtime {
    public class AdfinRealtime : IRealtime, ISupportsLogging {
        private readonly IContainer _container;
        private string _feed = "Q";
        private List<string> _rics;

        public AdfinRealtime(IContainer container) {
            _container = container;
            Logger = container.GetInstance<ILogger>();
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
            return _container
                        .With("rics").EqualTo(_rics)
                        .With("feed").EqualTo(_feed)
                        .GetInstance<ISubscriptionSetup>();
            //return new AdfinSubscriptionSetup(_rics, _feed);
        }

        public ILogger Logger { get; set; }
    }
}