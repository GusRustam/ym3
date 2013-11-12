using System.Collections.Generic;
using Toolbox.Async;

namespace DataProvider.Loaders.Chain.Data {
    public interface IChainRecord {
        string ChainRic { get;  }
        TimeoutStatus Status { get; }
        IList<string> Rics { get; }
    }
}