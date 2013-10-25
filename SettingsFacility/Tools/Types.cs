using System.Collections;
using System.Collections.Generic;
using NLog;

namespace SettingsFacility.Tools {
    public interface ISettingType<T> {
        T Value { get; set; }
    }

    public abstract class SettingTypeBase<TValueType, TDecoderType> : ISettingType<TValueType> 
        where TDecoderType : ISettingDecoder<TValueType>, new() {
        
        public abstract TValueType Value { get; set; }
        public static TDecoderType Decoder {
            get { return new TDecoderType(); }
        }
    }

    public sealed class LogLevelType : SettingTypeBase<LogLevel, LogLevelDecoder> {
        public override LogLevel Value { get; set; }
    }

    public sealed class DblType : SettingTypeBase<double?, DblDecoder> {
        private double? _value;

        public override double? Value {
            get { return _value; }
            set { _value = value; }
        }

        public DblType() {
            _value = default(double);
        }
    }

    public sealed class IntType : SettingTypeBase<int?, IntDecoder> {
        private int? _value;

        public override int? Value {
            get { return _value; }
            set { _value = value; }
        }

        public IntType() {
            _value = default(int);
        }
    }

    public sealed class StrType : SettingTypeBase<string, StringDecoder> {
        public override string Value { get; set; }
    }

    public sealed class BoolType : SettingTypeBase<bool?, BoolDecoder> {
        private bool? _value;

        public override bool? Value {
            get { return _value; }
            set { _value = value; }
        }

        public BoolType() {
            _value = default(bool);
        }
    }

    public abstract class ArrayType<T, Q> : SettingTypeBase<IEnumerable<T>, Q>
        where Q : ISettingDecoder<IEnumerable<T>>, new() {
    }

    public sealed class StringArrayType : ArrayType<string, StringArrayDecoder> {
        public override IEnumerable<string> Value { get; set; }
    }

    public sealed class DoubleArrayType : ArrayType<double?, DoubleArrayDecoder> {
        public override IEnumerable<double?> Value { get; set; }
    }

}
