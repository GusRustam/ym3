using System;

namespace DataProvider.Loaders.History.Data {
    public interface ITimeSeriesRecord : IHistoricalRecord {
        DateTime? Date { get; }
        double? Open { get; }
        double? High { get; }
        double? Low { get; }
        double? Close { get; }
    }
}