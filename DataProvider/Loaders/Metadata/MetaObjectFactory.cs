using DataProvider.Loaders.Metadata.Data;
using StructureMap;

namespace DataProvider.Loaders.Metadata {
    public class MetaObjectFactory<T> : IMetaObjectFactory<T> where T : IMetadataItem, new() {
        private readonly IContainer _container;

        public MetaObjectFactory(IContainer container) {
            _container = container;
        }

        public MetadataRequestAlgo<T> CreateAlgo(IRequestSetup<T> setup) {
            return _container.With(setup).GetInstance<MetadataRequestAlgo<T>>();
        }

        public IMetadataRequest<T> CreateRequest(IRequestSetup<T> setup) {
            return _container.With(setup).GetInstance<IMetadataRequest<T>>();
        }

        public IRequestSetup<T> CreateSetup() {
            return _container.GetInstance<IRequestSetup<T>>();
        }

        public IMetadataContainer<T> CreateContainer(IRequestSetup<T> setup) {
            return _container.With(setup).GetInstance<IMetadataContainer<T>>();
        }

        public IMetadataImporter<T> CreateImporter(IRequestSetup<T> setup) {
            return _container.With(setup).GetInstance<IMetadataImporter<T>>();
        }
    }
}