using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;

namespace SettingsFacility.Tools {
    public interface ISettingDecoder<T> {
        T Decode(string item);
        string Encode(T item);
    }

    public static class DecoderConstants {
        public const string NoValue = "!NA";
    }

    public abstract class SettingDecoderBase<T> : ISettingDecoder<T> {
        public abstract T Decode(string item);
        public abstract string Encode(T item);
    }

    public class LogLevelDecoder : SettingDecoderBase<LogLevel> {
        public override LogLevel Decode(string item) {
            return LogLevel.FromString(item);
        }

        public override string Encode(LogLevel item) {
            return item.ToString();
        }
    }

    public class DblDecoder : SettingDecoderBase<double?> {
        private string _format = "";
        public override double? Decode(string item) {
            double res;
            if (double.TryParse(item, NumberStyles.Any, CultureInfo.InvariantCulture, out res)) {
                return res;
            }
            return null;
        }

        public override string Encode(double? item) {
            if (!item.HasValue) return DecoderConstants.NoValue;

            var format = string.IsNullOrEmpty(_format) ? "{0}" : string.Format("{{0:{0}}}", _format);
            return string.Format(CultureInfo.InvariantCulture, format, item.Value);
        }

        public void SetFormat(string format) {
            _format = format;
        }
    }

    public class IntDecoder : SettingDecoderBase<int?> {
        public override int? Decode(string item) {
            int res;
            if (int.TryParse(item, NumberStyles.Any, CultureInfo.InvariantCulture, out res)) {
                return res;
            }
            return null;
        }

        public override string Encode(int? item) {
            return item.HasValue ? item.Value.ToString(CultureInfo.InvariantCulture) : DecoderConstants.NoValue;
        }
    }

    public class BoolDecoder : SettingDecoderBase<bool?> {
        public override bool? Decode(string item) {
            bool res;
            if (bool.TryParse(item, out res)) {
                return res;
            }
            return null;
        }

        public override string Encode(bool? item) {
            return item.HasValue ? item.Value.ToString() : DecoderConstants.NoValue;
        }
    }

    public class StringDecoder : SettingDecoderBase<string> {
        public override string Decode(string item) {
            return item;
        }

        public override string Encode(string item) {
            return item;
        }
    }

    // Милая штучка, позволяет объединять в списки любые декодируемые сущности
    public class ArrayDecoder<T, TDecoderType> : SettingDecoderBase<IEnumerable<T>> where TDecoderType : ISettingDecoder<T>, new() {

        public override IEnumerable<T> Decode(string item) {
            if (string.IsNullOrEmpty(item)) return new T[] {};
            
            item = Regex.Replace(item, @"\s+", "");
            if (item == "[]") return new T[] {};
            if (item[0] != '[') return new T[] { };
            if (item[item.Length-1] != ']') return new T[] { };

            item = Regex.Replace(item, @"[\[\]]", "");
            var decoder = new TDecoderType();
            return from elem in item.Split(',')
                let trimmed = elem.Trim()
                let decoded = decoder.Decode(trimmed)
                where decoded != null // todo cud it happen? I can't use T:class 'cos it kills nullables
                select decoded;
        }

        public override string Encode(IEnumerable<T> item) {
            var arr = item as T[] ?? item.ToArray();
            if (item == null || !arr.Any())
                return DecoderConstants.NoValue;
            
            var decoder = new TDecoderType();
            
            var decodeds = from elem in arr select decoder.Encode(elem);
            var res = string.Join(",", decodeds);
            return string.Format("[{0}]", res);
        }
    }

    public class StringArrayDecoder : ArrayDecoder<string, StringDecoder> {
    }

    public class DoubleArrayDecoder : ArrayDecoder<double?, DblDecoder> {
    }

    public class StringArrayDecoder1 : SettingDecoderBase<IEnumerable<string>> {
        public override IEnumerable<string> Decode(string item) {
            if (string.IsNullOrEmpty(item)) return new string[] {};
            return item.Split().ToList().Select(el => el.Trim());
        }

        public override string Encode(IEnumerable<string> item) {
            var arr = item as string[] ?? item.ToArray();
            if (item == null || !arr.Any()) 
                return DecoderConstants.NoValue;
            return arr.Aggregate("", (str, elem) => str + "," + elem);
        }
    }
}