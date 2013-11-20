using LoggingFacility;
using LoggingFacility.LoggingSupport;

namespace UnitTesting.Logging.UnitTesting {
    //[LoggerName(typeof(SomeAnnotatedLogga))]
    class SomeAnnotatedLogga : ISupportsLogging {
        public SomeAnnotatedLogga(ILogger logger) {
            Logger = logger;
        }

        public void Greet() {
            this.Info("Hello world");
        }

        public ILogger Logger { get; private set; }
    }
}
