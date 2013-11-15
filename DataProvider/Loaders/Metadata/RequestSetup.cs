using System;
using System.IO;
using System.Linq;

namespace DataProvider.Loaders.Metadata {
    public class RequestSetup<T> : RequestSetupBase, IRequestSetup<T> where T : IMetadataItem, new() {
        private string[] _fields = {};
        public string[] Fields {
            get {
                if (_fields.Any() || FieldInfo == null)
                    return _fields;
                
                _fields = FieldInfo
                    .Where(x => !string.IsNullOrEmpty(x.MetaFieldName))
                    .Select(x => x.MetaFieldName)
                    .Distinct()
                    .ToArray();
                
                return _fields;
            }
        }

        private MetaVariableInfo[] _vars = {};
        public MetaVariableInfo[] Varaibles {
            get {
                if (!_vars.Any() || FieldInfo == null)
                    return _vars;

                _vars = FieldInfo
                    .Select(x => new MetaVariableInfo {
                        VariableName = x.VariableName,
                        VariableType = x.VariableType
                    })
                    .Distinct()
                    .ToArray();

                return _vars;
            }
        }

        public string[] Rics { get; set; }
        public Action<IMetadataContainer<T>> Callback { get; set; }

        public RequestSetup(IMetadataExporter<T> exporter) {
            var setup = exporter.GetMetaParams();
            if (setup == null) throw new InvalidDataException("data");

            DisplayMode = setup.DisplayMode;
            RequestMode = setup.RequestMode;
            FieldInfo = setup.FieldInfo;
        }
    }
}