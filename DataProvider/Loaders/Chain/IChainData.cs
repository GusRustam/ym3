using System.Collections.Generic;
using DataProvider.Loaders.History.Data;
using DataProvider.Loaders.Status;

namespace DataProvider.Loaders.Chain {
    public interface IChainData {
        IDictionary<string, IList<string>> Data { get;  }
    }
}