using System;
using System.IO;
using System.Linq;

namespace DataProvider.Loaders.Metadata {
    public class RequestSetup<T> : RequestSetupBase, IRequestSetup<T> where T : IMetadataItem {
        public string[] Fields {
            get {
                return FieldInfo == null 
                    ? new string[] {} 
                    : FieldInfo.Select(x => x.MetaFieldName).Distinct().ToArray();
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