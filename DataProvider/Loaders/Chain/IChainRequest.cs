using System.Collections.Generic;
using Toolbox.Async;

namespace DataProvider.Loaders.Chain {
    public interface IChainRequest : ITimeoutCall {
        IList<string> Rics { get; }
    }
}