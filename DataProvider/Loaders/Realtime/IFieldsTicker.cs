using System;
using DataProvider.Loaders.Realtime.Data;
using Toolbox.Async;

namespace DataProvider.Loaders.Realtime {
    public interface IFieldsTicker : ITimeout {
        IFieldsTicker WithFields(Action<IRicsFields> callback);
    }
}