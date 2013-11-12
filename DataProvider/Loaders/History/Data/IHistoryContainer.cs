using System;
using System.Collections.Generic;
using DataProvider.Storage;
using Toolbox.Async;

namespace DataProvider.Loaders.History.Data {
    public interface IHistoryContainer : IStorage<string, DateTime, IHistoryField, string> {
        TimeoutStatus Status { get; set; }
        IDictionary<string, TimeoutStatus> RicStatuses { get; }
        IHistoryContainer Import(IHistoryContainer container);
    }
}