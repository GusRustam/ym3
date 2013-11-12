using System.Collections.Generic;
using Toolbox.Async;

namespace DataProvider.Loaders.Chain.Data {
    public class ChainResponse : IChainResponse {
        private readonly IList<IChainRecord> _records = new List<IChainRecord>();

        public TimeoutStatus Status { get; set; }

        public IList<IChainRecord> Records {
            get { return _records; }
        }
    }
}