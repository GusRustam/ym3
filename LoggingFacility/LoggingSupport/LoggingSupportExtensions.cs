using System;
using System.Diagnostics;
using NLog;

namespace LoggingFacility.LoggingSupport {
    public static class LoggingSupportExtensions {
        public static LogLevel AsNLevel(this Level level) {
            if (level == Level.Off)
                return LogLevel.Off;
            if (level == Level.Trace)
                return LogLevel.Trace;
            if (level == Level.Debug)
                return LogLevel.Debug;
            if (level == Level.Info)
                return LogLevel.Info;
            if (level == Level.Warn)
                return LogLevel.Warn;
            if (level == Level.Error)
                return LogLevel.Error;
            return level == Level.Fatal ? LogLevel.Fatal : null;
        }

        [Conditional("DEBUG")]
        public static void Trace(this ISupportsLogging logger, string msg, Exception ex = null) {
            Log(logger, Level.Trace, msg, ex);
        }

        [Conditional("DEBUG")]
        public static void Debug(this ISupportsLogging logger, string msg, Exception ex = null) {
            Log(logger, Level.Debug, msg, ex);
        }

        [Conditional("DEBUG")]
        public static void Info(this ISupportsLogging logger, string msg, Exception ex = null) {
            Log(logger, Level.Info, msg, ex);
        }

        [Conditional("DEBUG")]
        public static void Warn(this ISupportsLogging logger, string msg, Exception ex = null) {
            Log(logger, Level.Warn, msg, ex);
        }

        [Conditional("DEBUG")]
        public static void Error(this ISupportsLogging logger, string msg, Exception ex = null) {
            Log(logger, Level.Error, msg, ex);
        }

        [Conditional("DEBUG")]
        public static void Fatal(this ISupportsLogging logger, string msg, Exception ex = null) {
            Log(logger, Level.Fatal, msg, ex);
        }

        [Conditional("DEBUG")]
        private static void Log(ISupportsLogging logger, Level level, string msg, Exception ex = null) {
            var l = logger.Logger;
            if (level >= l.Threshold)
                l.Log(level, msg, ex);
        }
    }
}