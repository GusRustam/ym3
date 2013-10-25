using System;

namespace DataProvider.DataLoaders {
    public interface ITimeout {
        ITimeout WithTimeout(Action callback);
        void Request();
    }
}