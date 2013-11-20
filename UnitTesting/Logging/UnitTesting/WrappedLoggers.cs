using System.Collections;
using System.Collections.Generic;
using LoggingFacility;
using LoggingFacility.Loggers;

namespace UnitTesting.Logging.UnitTesting {
    public class WrappedLoggers : IEnumerable<object[]> {
        private readonly List<object[]> _data = new List<object[]> {
            new object[] { new LoggerWrapper(new ConsoleLogger(Level.Info, "Vova")) },
            new object[] { new LoggerWrapper(new NLogLogger(Level.Info, "Vova")) }

        };
        public IEnumerator<object[]> GetEnumerator() {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}