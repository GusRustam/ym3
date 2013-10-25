using System;

namespace LoggingFacility {
    public interface ILogger {
        Level Threshold { get; set; }
        string Name { get;  }
        void Log(Level level, string msg, Exception ex = null);
    }
}