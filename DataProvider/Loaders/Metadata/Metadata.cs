using System;

namespace DataProvider.Loaders.Metadata {
    public class Metadata : IMetadata {
        private readonly IMetaObjectFactory _factory;
        public string[] Rics { get; private set; }

        public IMetadata WithRics(params string[] rics) {
            Rics = rics;
            return this;
        }

        public IMetadataReciever<TObject> Reciever<TObject>() where TObject : IMetadataFields, new() {
            return _factory.CreateReciever<TObject>();
        }

        public IMetadataReciever Reciever(Type type) {
            return _factory.CreateReciever();
        }

        public IMetadataRequest Request() {
            return _factory.CreateRequest(this);
        }

        public Metadata(IMetaObjectFactory factory) {
            _factory = factory;
        }
    }
}