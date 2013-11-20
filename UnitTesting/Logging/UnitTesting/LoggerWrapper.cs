using System;
using LoggingFacility;
using LoggingFacility.Loggers;

namespace UnitTesting.Logging.UnitTesting {
    public class LoggerWrapper : LoggerBase {
        private readonly ILogger _logger;
        public event Action<Level, string, Exception> Logged;
        public LoggerWrapper(ILogger logger)
            : base(logger.Threshold, logger.Name) {
            if (logger == null)
                throw new Exception();
            _logger = logger;
        }

        public override void Log(Level level, string msg, Exception ex = null) {
            _logger.Log(level, msg, ex);
            if (Logged != null)
                Logged(level, msg, ex);
        }
    }
}