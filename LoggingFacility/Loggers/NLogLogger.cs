using System;
using LoggingFacility.LoggingSupport;
using NLog;

namespace LoggingFacility.Loggers {
    public sealed class NLogLogger : LoggerBase {
        private readonly Logger _logger;

        public NLogLogger(Level threshold, string name) : base(threshold, name) {
            _logger = NLogManager.Instance.GetLogger(name);
        }

        public override void Log(Level level, string msg, Exception ex = null) {
            var nLevel = level.AsNLevel();
            _logger.Log(nLevel, msg);
            if (ex != null)
                _logger.Log(nLevel, "Exception = {0}", ex.ToString());
        }
    }
}
