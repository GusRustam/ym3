using System;
using DataProvider.Loaders.Metadata.Data;

namespace DataProvider.Loaders.Metadata {
    public class Metadata<T> : IMetadata<T> where T : IMetadataItem, new() {
        private readonly IMetaObjectFactory<T> _factory;
        private readonly IRequestSetup<T> _setup;

        public IMetadata<T> WithRics(params string[] rics) {
            _setup.Rics = rics;
            return this;
        }

        public IMetadata<T> OnFinished(Action<IMetadataContainer<T>> action) {
            _setup.Callback = action;
            return this;
        }

        public IMetadataRequest<T> Request() {
            return _factory.CreateRequest(_setup);
        }

        public Metadata(IMetaObjectFactory<T> factory) {
            _factory = factory;
            _setup = factory.CreateSetup();
        }
    }
}