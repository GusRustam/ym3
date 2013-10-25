using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LoggingFacility.LoggingSupport;

namespace LoggingFacility {
    public class LoggingContext : ILoggingContext {
        private readonly IList<ILogger> _loggers = new List<ILogger>();
        private Level _globalThreshold = Level.Trace;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RegisterLogger(ILogger logger) {
            _loggers.Add(logger);
        }

        public Level GlobalThreshold {
            get { return _globalThreshold; }
            set {
                _globalThreshold = value;
                NLogManager.Instance.LoggingLevel = value.AsNLevel();
                foreach (var l in _loggers) l.Threshold = _globalThreshold;
            }
        }
    }
}