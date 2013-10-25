using System;

namespace DataProvider.DataLoaders {
    public interface IFieldsTicker : ITimeout {
        IFieldsTicker WithFields(Action<IRicsFields> callback);
    }
}