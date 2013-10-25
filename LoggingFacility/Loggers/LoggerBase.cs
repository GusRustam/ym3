using System;

namespace LoggingFacility.Loggers {
    public abstract class LoggerBase : ILogger {
        protected LoggerBase(Level threshold, string name) {
            Name = name;
            Threshold = threshold;
        }

        public Level Threshold { get; set; }
        public string Name { get; private set; }
        public abstract void Log(Level level, string msg, Exception ex = null);
    }
}