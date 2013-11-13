using StructureMap;

namespace DataProvider.Loaders.Metadata {
    public class MetaObjectFactory<T> : IMetaObjectFactory<T> where T : IMetadataItem, new() {
        private readonly IContainer _container;

        public MetaObjectFactory(IContainer container) {
            _container = container;
        }

        public MetadataRequest<T>.MetadataRequestAlgo CreateAlgo(IMetaRequestSetup<T> setup) {
            return _container.With(setup).GetInstance<MetadataRequest<T>.MetadataRequestAlgo>();
        }

        public IMetadataRequest<T> CreateRequest(IMetaRequestSetup<T> setup) {
            return _container.With(setup).GetInstance<IMetadataRequest<T>>();
        }

        public IMetaRequestSetup<T> CreateSetup() {
            return _container.GetInstance<IMetaRequestSetup<T>>();
        }

        public IMetadataContainer<T> CreateContainer() {
            return _container.GetInstance<IMetadataContainer<T>>();
        }
    }
}