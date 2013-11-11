using System;
using DataProvider.Storage;

namespace DataProvider.Loaders.History.Data {
    public interface IHistoryContainer : IStorage<string, DateTime, IHistoryField, string> {
        IHistoryContainer Import(IHistoryContainer container);
    }
}