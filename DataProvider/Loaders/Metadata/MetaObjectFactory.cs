using StructureMap;

namespace DataProvider.Loaders.Metadata {
    public class MetaObjectFactory : IMetaObjectFactory {
        private readonly IContainer _container;

        public MetaObjectFactory(IContainer container) {
            _container = container;
        }

        public IMetadataReciever<T> CreateReciever<T>() where T : IMetadataFields, new() {
            return _container.GetInstance<IMetadataReciever<T>>();
        }

        public IMetadataReciever CreateReciever() {
            return _container.GetInstance<IMetadataReciever>();
        }

        public IMetadataRequest CreateRequest(IMetadata metadata) {
            return _container.GetInstance<IMetadataRequest>();
        }

        public MetadataRequest.MetadataRequestAlgo CreateAlgo(IMetaRequestSetup setup) {
            return _container
                .With(setup)
                .GetInstance<MetadataRequest.MetadataRequestAlgo>();
        }
    }
}