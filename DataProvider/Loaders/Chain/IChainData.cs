using System.Collections.Generic;
using DataProvider.Loaders.Status;

namespace DataProvider.Loaders.Chain {
    public interface IChainData {
        string ChainRic { get; set; }
        IEnumerable<string> Rics { get; set;  }
    }

    public class ChainData : IChainData {
        public string ChainRic { get; set; }
        public IEnumerable<string> Rics { get; set; }
    }
}