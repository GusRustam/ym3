using System.Collections.Generic;
using Toolbox.Async;

namespace DataProvider.Loaders.Chain.Data {
    public class ChainRecord : IChainRecord {
        public string ChainRic { get; private set; }
        public TimeoutStatus Status { get; private set; }
        public IList<string> Rics { get; private set; }

        public ChainRecord(string chainRic, TimeoutStatus status, IList<string> rics) {
            ChainRic = chainRic;
            Status = status;
            Rics = rics;
        }
    }
}