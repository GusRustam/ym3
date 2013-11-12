using System;

namespace Toolbox.Async {
    public class TimeoutStatus {
        private readonly string _name;
        public static TimeoutStatus Ok = new TimeoutStatus("Ok");
        public static TimeoutStatus Timeout = new TimeoutStatus("Timeout");
        public static TimeoutStatus Cancelled = new TimeoutStatus("Cancelled");
        public static TimeoutStatus Error = new ErrorStatus("Error");

        public class ErrorStatus : TimeoutStatus {
            public Exception Err { get; internal set; }

            public ErrorStatus(string name) : base(name) {
            }

            public override string ToString() {
                return Err == null ? base.ToString() : Err.ToString();
            }
        }

        private TimeoutStatus(string name) {
            _name = name;
        }

        public override string ToString() {
            return _name;
        }

        protected bool Equals(TimeoutStatus other) {
            return string.Equals(_name, other._name);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((TimeoutStatus) obj);
        }

        public override int GetHashCode() {
            return (_name != null ? _name.GetHashCode() : 0);
        }

        public static bool operator ==(TimeoutStatus left, TimeoutStatus right) {
            return Equals(left, right);
        }

        public static bool operator !=(TimeoutStatus left, TimeoutStatus right) {
            return !Equals(left, right);
        }

        public static TimeoutStatus CreateError(Exception exception) {
            return new ErrorStatus("Error") { Err = exception };
        }
    }
}