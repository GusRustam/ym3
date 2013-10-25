using System;

namespace LoggingFacility.Loggers {
    public sealed class ConsoleLogger : LoggerBase {
        public ConsoleLogger(Level threshold, string name) : base(threshold, name) {
        }

        public override void Log(Level level, string msg, Exception ex = null) {
            Console.WriteLine("{0} | {3} | {1:dd-MM-yy hh:mm:ss.fff} | {2}",
                level.ToString().ToUpper(), DateTime.Now, msg, Name);
            if (ex != null)
                Console.WriteLine(ex.ToString());
        }
    }
}