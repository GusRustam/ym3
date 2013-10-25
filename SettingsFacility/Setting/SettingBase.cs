using System;
using System.Text;
using System.Xml.Linq;

namespace SettingsFacility.Setting {
    public abstract class SettingBase : ISetting, IEquatable<SettingBase> {
        protected readonly GroupSetting Parent;

        #region Equality members
        public bool Equals(SettingBase other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Parent, other.Parent) && string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((SettingBase) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((Parent != null ? Parent.GetHashCode() : 0)*397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }

        public static bool operator ==(SettingBase left, SettingBase right) {
            return Equals(left, right);
        }

        public static bool operator !=(SettingBase left, SettingBase right) {
            return !Equals(left, right);
        }
        #endregion

        internal abstract XElement ToXml();
        internal abstract void ReadXml(XElement elem);

        public virtual string Name { get; private set; }
        public virtual string XmlName { get; private set; }
        public virtual string Description { get; private set; }

        public string QualifiedName {
            get {
                var res = new StringBuilder();
                var current = this;
                while (current != null) {
                    res.Append(current.XmlName).Append("/");
                    current = current.Parent;
                }
                return res.ToString(0, res.Length - 1);
            }
        }

        public abstract bool Group { get; }

        public event Action<ISetting> Changed;
        protected void NotifyChanged(ISetting setting) {
            Changed(setting);
        }

        protected SettingBase(string name, string xmlName, string description, GroupSetting parent) {
            Description = description;
            XmlName = xmlName;
            Name = name;
            Parent = parent;
        }
    }
}