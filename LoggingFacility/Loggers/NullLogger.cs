using System;

namespace LoggingFacility.Loggers {
    public sealed class NullLogger : ILogger {
        public Level Threshold { get; set; }
        public string Name { get; set; }

        public void Log(Level level, string msg, Exception ex = null) {
        }
    }
}