using System;

namespace DataProvider.Loaders.Metadata.Data {
    public struct MetaFieldInfo {
        public int Order;
        public string MetaFieldName;
        public Type VariableType;
        public string VariableName;
        public Type Converter;
    }
}