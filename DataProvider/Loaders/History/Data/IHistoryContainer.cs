using System;

namespace DataProvider.Loaders.History.Data {
    public interface IHistoryContainer : IStorage<string, DateTime, IHistoryField, string> {
        IHistoryContainer Import(IHistoryContainer container);
    }
}