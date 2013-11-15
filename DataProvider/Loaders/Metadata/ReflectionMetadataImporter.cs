using System;
using System.Collections.Generic;
using LoggingFacility;
using LoggingFacility.LoggingSupport;

namespace DataProvider.Loaders.Metadata {
    public class ReflectionMetadataImporter<T> : ISupportsLogging, IMetadataImporter<T> where T : IMetadataItem, new() {
        private readonly MetaFieldInfo[] _fields;
        // ReSharper disable StaticFieldInGenericType
        private static readonly Dictionary<Type, object> Converters;
        // ReSharper restore StaticFieldInGenericType

        static ReflectionMetadataImporter() {
            Converters = new Dictionary<Type, object>();
        } 

        public ReflectionMetadataImporter(IRequestSetup<T> setup, ILogger logger) {
            Logger = logger;
            _fields = setup.FieldInfo;
        }

        public T Import(object[] currentRow) {
            var res = new T();
            var type = typeof(T);
            foreach (var item in _fields) {
                var value = currentRow[item.Order];
                this.Trace(string.Format("Value {0} -> variable {1} of type {2}", value, item.VariableName, item.VariableType.Name));
                
                var property = type.GetProperty(item.VariableName);
                
                if (item.Converter != null) {
                    IMetadataConverter converter = null;

                    // first let us see if we have already found such converters
                    if (!Converters.ContainsKey(item.Converter)) {
                        // no, we didn't. Creating it
                        converter = Activator.CreateInstance(item.Converter) as IMetadataConverter;

                        // failed we or not, we store the result - either null or object
                        Converters[item.Converter] = converter;
                    }

                    // now there must be something in the dictionary
                    if (Converters[item.Converter] != null)
                        converter = Converters[item.Converter] as IMetadataConverter;

                    // and if it was not null and it was a valid object, let's apply it
                    if (converter != null) value = converter.Decode(value.ToString());
                }
                property.SetValue(res, value, null);
            }
            return res;
        }

        public ILogger Logger { get; private set; }
    }
}