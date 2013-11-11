using System.Collections.Generic;

namespace DataProvider.Loaders.Chain {
    public class ChainData : IChainData {
        private readonly IDictionary<string, IList<string>> _data;

        public ChainData() {
            _data = new Dictionary<string, IList<string>>();
        }

        public IDictionary<string, IList<string>> Data {
            get { return _data; }
        }
    }
}