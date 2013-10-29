using System.Collections.Generic;
using DataProvider.Loaders.Status;

namespace DataProvider.Loaders.Realtime.Data {
    public interface IRicsFields {
        ISourceStatus SourceStatus { get;  }
        Dictionary<string, IRicFields> Data { get;  }
    }
}