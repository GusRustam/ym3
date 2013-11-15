using System;
using DataProvider.Loaders.Metadata.Data;
using Toolbox.Async;

namespace DataProvider.Loaders.Metadata {
    public class MetadataRequest<T> : IMetadataRequest<T> where T : IMetadataItem, new() {
        private readonly MetadataRequestAlgo<T> _algo;

        
        public MetadataRequest(IMetaObjectFactory<T> factory, IRequestSetup<T> setup) {
            _algo = factory.CreateAlgo(setup);
        }

        public ITimeoutCall WithTimeout(TimeSpan? timeout) {
            _algo.WithTimeout(timeout);
            return this;
        }

        public void Request() {
            _algo.Request();
        }

        public void Cancel() {
            _algo.Cancel();
        }
    }
}