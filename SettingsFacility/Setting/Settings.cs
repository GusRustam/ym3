using System.Collections.Generic;
using NLog;
using SettingsFacility.Tools;

namespace SettingsFacility.Setting {
    public sealed class DoubleSetting : UnitSetting<DblType, double?, DblDecoder> {
        public DoubleSetting(string name, string xmlName, string description, double value = default(double), GroupSetting parent = null) :
                base(name, xmlName, description, value, parent) {
        }

        public string SaveFormat {
            set { Decoder.SetFormat(value); }
        }
    }

    public sealed class IntegerSetting : UnitSetting<IntType, int?, IntDecoder> {
        public IntegerSetting(string name, string xmlName, string description, int value = default(int), GroupSetting parent = null) :
            base(name, xmlName, description, value, parent) {
        }
    }

    public sealed class BooleanSetting : UnitSetting<BoolType, bool?, BoolDecoder> {
        public BooleanSetting(string name, string xmlName, string description, bool value = default(bool), GroupSetting parent = null) :
            base(name, xmlName, description, value, parent) {
        }
    }

    public sealed class LoggerLevelSetting : UnitSetting<LogLevelType, LogLevel, LogLevelDecoder> {
        public LoggerLevelSetting(string name, string xmlName, string description, LogLevel value = null, GroupSetting parent = null) :
            base(name, xmlName, description, value, parent) {
        }
    }

    public sealed class StringSetting : UnitSetting<StrType, string, StringDecoder> {
        public StringSetting(string name, string xmlName, string description, string value = "", GroupSetting parent = null) :
            base(name, xmlName, description, value, parent) {
        }
    }

    public sealed class StringArrSetting : UnitSetting<StringArrayType, IEnumerable<string>, StringArrayDecoder> {
        public StringArrSetting(string name, string xmlName, string description, IEnumerable<string> value, GroupSetting parent = null) : 
            base(name, xmlName, description, value, parent) {
        }
    }

    public sealed class DoubleArrSetting : UnitSetting<DoubleArrayType, IEnumerable<double?>, DoubleArrayDecoder> {
        public DoubleArrSetting(string name, string xmlName, string description, IEnumerable<double?> value, GroupSetting parent = null) :
            base(name, xmlName, description, value, parent) {
        }
    }
}
