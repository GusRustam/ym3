using System;

namespace DataProvider.Loaders.Realtime {
    public interface ITimeout {
        ITimeout WithTimeout(Action callback);
        void Request();
    }
}