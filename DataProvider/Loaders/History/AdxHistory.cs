using System;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using StructureMap;

namespace DataProvider.Loaders.History {
    // todo - I could get rid of Adx prefix if I used DI here
    public class History : IHistory, ISupportsLogging {
        private string _ric;
        private readonly IContainer _container;

        public History(IContainer container) {
            _container = container;
            Logger = container.GetInstance<ILogger>();
        }

        public IHistoryRequest Subscribe(string ric) {
            _ric = ric;
            Validate();
            return _container.With("ric").EqualTo(ric).GetInstance<IHistoryRequest>();
        }

        private void Validate() {
            if (string.IsNullOrEmpty(_ric)) throw new ArgumentException("ric");
        }

        public ILogger Logger { get; private set; }
    }
}