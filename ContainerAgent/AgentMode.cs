namespace ContainerAgent {
    public class AgentMode {
        public static AgentMode External = new AgentMode("External");
        public static AgentMode InEikon = new AgentMode("InEikon");
        public static AgentMode Default = External;
        private readonly string _mode;

        private AgentMode(string mode) {
            _mode = mode;
        }

        public override string ToString() {
            return _mode;
        }

        protected bool Equals(AgentMode other) {
            return string.Equals(_mode, other._mode);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((AgentMode) obj);
        }

        public override int GetHashCode() {
            return _mode.GetHashCode();
        }

        public static bool operator ==(AgentMode left, AgentMode right) {
            return Equals(left, right);
        }

        public static bool operator !=(AgentMode left, AgentMode right) {
            return !Equals(left, right);
        }

        public static implicit operator string(AgentMode @this) {
            return @this.ToString();
        }
    }
}