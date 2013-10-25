using System;

namespace LoggingFacility.LoggingSupport {
    public class LoggerName : Attribute {
        private readonly string _name;

        public LoggerName(string name) {
            _name = name;
        }
        public LoggerName(Type type) {
            _name = type.ToString();
        }

        public string Name {
            get { return _name; }
        }
    }
}