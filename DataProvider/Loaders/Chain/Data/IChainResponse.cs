using System.Collections.Generic;
using Toolbox.Async;

namespace DataProvider.Loaders.Chain.Data {
    public interface IChainResponse {
        TimeoutStatus Status { get; set; }
        IList<IChainRecord> Records { get; }
    }
}