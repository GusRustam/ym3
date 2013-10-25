using System.Collections.Generic;
using DataProvider.DataLoaders.Status;

namespace DataProvider.DataLoaders {
    public interface IRicsFields {
        ISourceStatus SourceStatus { get;  }
        Dictionary<string, IRicFields> Data { get;  }
    }
}