using System;
using System.Linq;
using ThomsonReuters.Interop.RTX;

namespace DataProvider.DataLoaders.Status {
    public class RunMode : IRunMode {
        protected bool Equals(RunMode other) {
            return string.Equals(_name, other._name);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((RunMode) obj);
        }

        public override int GetHashCode() {
            return (_name != null ? _name.GetHashCode() : 0);
        }

        private readonly string _name;
        public static readonly IRunMode OnTimeIfUpdated = new RunMode("OnTimeIfUpdated");
        public static readonly IRunMode OnTime = new RunMode("OnTime");
        public static readonly IRunMode OnUpdate = new RunMode("OnUpdate");
        public static readonly IRunMode Snapshot = new RunMode("Snapshot");
        public static readonly IRunMode Unknown = new RunMode("Unknown");

        private static readonly IRunMode[] Statuses = { OnTimeIfUpdated, OnTime, OnUpdate, Snapshot, Unknown };

        private RunMode(string name) {
            _name = name;
        }

        public static IRunMode FromAdxStatus(RT_RunMode status) {
            switch (status) {
                case RT_RunMode.RT_MODE_ONTIME_IF_UPDATED:
                    return OnTimeIfUpdated;

                case RT_RunMode.RT_MODE_ONTIME:
                    return OnTime;

                case RT_RunMode.RT_MODE_ONUPDATE:
                    return OnUpdate;

                case RT_RunMode.RT_MODE_IMAGE:
                    return Snapshot;

                case RT_RunMode.RT_MODE_NOT_SET:
                    return Unknown;

                default:
                    return null;
            }
        }

        public static IRunMode FromString(string name) {
            var stringStatuses = Statuses.Select(status => status.ToString()).ToArray();
            var pos = Array.IndexOf(stringStatuses, name);
            return pos < 0 ? null : Statuses[pos];
        }

        public override string ToString() {
            return _name;
        }

        public RT_RunMode ToAdxMode() {
            if (Equals(OnTimeIfUpdated))
                return RT_RunMode.RT_MODE_ONTIME_IF_UPDATED;

            if (Equals(OnTime))
                return RT_RunMode.RT_MODE_ONTIME;

            if (Equals(OnUpdate))
                return RT_RunMode.RT_MODE_ONUPDATE;

            return Equals(Snapshot) ? RT_RunMode.RT_MODE_IMAGE : RT_RunMode.RT_MODE_NOT_SET;
        }
    }
}