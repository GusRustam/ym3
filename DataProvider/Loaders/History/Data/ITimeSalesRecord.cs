using System;

namespace DataProvider.Loaders.History.Data {
    public interface ITimeSalesRecord : IHistoricalRecord {
        double? Ask { get; }
        double? AskSize { get; }
        double? Bid { get; }
        double? BidSize { get; }
        DateTime Date { get; }
        string ExchangeSequence { get; }
        DateTime? ExchangeTime { get; }
        int? PriceTick { get; }
        string Tag { get; }
        double? Value { get; }
        string VenueId { get; }
        double? Vwap { get; }   
    }
}