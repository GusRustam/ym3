using System;

namespace DataProvider.Loaders.Metadata {
    public class MetadataReciever : IMetadataReciever {
        private readonly IMetadata _parent;

        public Action<object> Action { get; private set; }

        public MetadataReciever(IMetadata parent) {
            _parent = parent;
        }


        public IMetadata OnFinished(Action<object> action) {
            Action = action;
            return _parent;
        }
    }

    public class MetadataReciever<T> : IMetadataReciever<T> where T : IMetadataFields {
        private readonly IMetadata _parent;

        public Action<IMetadataContainer<T>> Action { get; private set; }

        public MetadataReciever(IMetadata parent) {
            _parent = parent;
        }

        public IMetadata OnFinished(Action<IMetadataContainer<T>> action) {
            Action = action;
            return _parent;
        }
    }
}