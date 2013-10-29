using System;
using DataProvider.Objects;
using LoggingFacility;
using LoggingFacility.LoggingSupport;

namespace DataProvider.Loaders.History {
    // todo - I could get rid of Adx prefix if I used DI here
    public class AdxHistory : IHistory, ISupportsLogging {
        private readonly IEikonObjects _objects;
        private string _ric;

        public AdxHistory(ILogger logger, IEikonObjects objects) {
            _objects = objects;
            Logger = logger;
        }

        public IHistoryRequest Subscribe(string ric) {
            _ric = ric;
            Validate();
            return new AdxHistoryRequest(_objects, Logger, ric);
        }

        private void Validate() {
            if (string.IsNullOrEmpty(_ric)) throw new ArgumentException("ric");
        }

        public ILogger Logger { get; private set; }
    }
}