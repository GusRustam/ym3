using System.IO;
using System.Xml.Linq;
using SettingsFacility.Tools;

namespace SettingsFacility.Setting {
    public abstract class UnitSetting<TSettingType, TValueType, TDecoderType> : SettingBase 
        where TSettingType : class, ISettingType<TValueType>, new()
        where TDecoderType : class, ISettingDecoder<TValueType>, new() {
        
        private readonly TSettingType _setting;
        private readonly TDecoderType _decoder;

        public TValueType Value { 
            get { return _setting.Value; }
            set {
                var oldVal = _setting.Value;
                _setting.Value = value;
                if (!Equals(oldVal, value)) NotifyChanged(this);
            } 
        }

        public static implicit operator TValueType(UnitSetting<TSettingType, TValueType, TDecoderType> x) {
            return x.Value;
        }

        public override string ToString() {
            return string.Format("{0} {1} of {2}", Name, Value, Value.GetType());
        }

        public override bool Group { get { return false; } }

        private readonly TValueType _defaultValue;
        public TValueType DefaultValue {
            get { return _defaultValue;  }
        }

        protected UnitSetting(string name, string xmlName, string description, TValueType value, GroupSetting parent = null) :
            base(name, xmlName, description, parent) {
            _setting = new TSettingType {Value = value};
            _decoder = new TDecoderType();
            _defaultValue = value;
        }

        internal override XElement ToXml() {
            return new XElement(XmlName, 
                new XAttribute("name", Name),
                new XAttribute("descr", Description),
                new XAttribute("value", Decoder.Encode(Value)));
        }

        internal override void ReadXml(XElement elem) {
            var xName = elem.Attribute("name").Value;
            if (Name != xName) throw new InvalidDataException(string.Format("Supplied XElement name {0} differs from expected name {1}", xName, Name));
            Value = Decoder.Decode(elem.Attribute("value").Value);
        }

        public TDecoderType Decoder {
            get { return _decoder; } 
        }
    }
}