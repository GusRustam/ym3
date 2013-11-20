using LoggingFacility;
using LoggingFacility.LoggingSupport;

namespace UnitTesting.Logging.UnitTesting {
    class SomeLogga : ISupportsLogging {
        public SomeLogga(ILogger logger) {
            Logger = logger;
        }

        public void Greet() {
            this.Info("Hello world");
        }

        public ILogger Logger { get; private set; }
    }
}