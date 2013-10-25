using System.Collections.Generic;
using DataProvider.DataLoaders.Status;

namespace DataProvider.DataLoaders {
    public class RicsFields : IRicsFields {
        public RicsFields(ISourceStatus sourceStatus, Dictionary<string, IRicFields> data) {
            SourceStatus = sourceStatus;
            Data = data;
        }

        public RicsFields(ISourceStatus sourceStatus) {
            SourceStatus = sourceStatus;
            Data = null;
        }

        public ISourceStatus SourceStatus { get; private set; }
        public Dictionary<string, IRicFields> Data { get; private set; }
    }
}