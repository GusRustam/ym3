using System;

namespace DataProvider.Loaders {
    public interface ITimeout {
        ITimeout WithTimeout(Action callback);
        void Request();
    }
}