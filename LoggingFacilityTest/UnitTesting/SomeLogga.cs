using LoggingFacility;
using LoggingFacility.LoggingSupport;

namespace LoggingFacilityTest.UnitTesting {
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