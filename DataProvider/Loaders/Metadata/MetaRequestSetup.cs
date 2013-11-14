using System;
using System.Collections.Generic;

namespace DataProvider.Loaders.Metadata {
    public class MetaRequestSetupBase {
        public string[] Fields { get; set; }
        public string DisplayMode { get; set; }
        public string RequestMode { get; set; }
    }

    public class MetaRequestSetup<T> : MetaRequestSetupBase, IMetaRequestSetup<T> where T : IMetadataItem {
        public string[] Rics { get; set; }
        public Action<IMetadataContainer<T>> Callback { get; set; }

        public MetaRequestSetup(IMetadataExporter<T> exporter) {
            var setup = exporter.GetMetaParams();
            DisplayMode = setup.DisplayMode;
            RequestMode = setup.RequestMode;
            Fields = setup.Fields;
        }
    }

    public interface IMetadataExporter<T> where T : IMetadataItem {
        MetaRequestSetupBase GetMetaParams();
    }

    public class MetadataExporter<T> : IMetadataExporter<T> where T : IMetadataItem {
        // ReSharper disable StaticFieldInGenericType - that's ok, it stores non-generic data from generic source
        private static readonly Dictionary<Type, MetaRequestSetupBase> FieldCache;
        // ReSharper restore StaticFieldInGenericType

        static MetadataExporter() {
            FieldCache = new Dictionary<Type, MetaRequestSetupBase>();
        }

        public MetaRequestSetupBase GetMetaParams() {
            var type = typeof (T);
            if (FieldCache.ContainsKey(type)) return FieldCache[type];

            // parsing now
            // it's a todo!!!

        }

    }
}