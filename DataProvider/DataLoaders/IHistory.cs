using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataProvider.DataLoaders {
    interface IHistory {
    }
    /* public interface ITimeSeriesRecord {
      DateTime? Date { get; }
      double? Open { get; }
      double? High { get; }
      double? Low { get; }
      double? Close { get; }
  }

  public interface ITimeSalesRecord {
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

  public interface IHistoryRequest {
        
  }

  public interface ITimeSeriesRequest {
         
  }

  public interface ITimeSalesRequest {
         
  }

  public enum HistoryInterval {
        
  }

      // todo this must be an overhead (?)
      //IHistoryRequestSetup JoinRecords();
      //IHistoryRequestSetup OnAllData();
  public interface IHistoryRequestSetup {
      IHistoryRequestSetup WithInterval(HistoryInterval interval);
      IHistoryRequestSetup Since(DateTime date);
      IHistoryRequestSetup Till(DateTime date);
      IHistoryRequestSetup NumRecords(int num);

      IHistoryRequestSetup OnDataChunk();
      IHistoryRequestSetup OnDataChunkFinished();
  }

  public interface ITimeSeriesRequestSetup : IHistoryRequestSetup {
  }

  public interface ITimeSalesRequestSetup : IHistoryRequestSetup {
  }

  public interface IHistory {
      ITimeSeriesRequestSetup CreateTimeSeriesRequest(params string[] rics);
      IHistoryRequestSetup CreateTimeSalesRequest(params string[] rics);
  }

  public class AdfinRealtime {
        
  }

  public class History {
        
  }

  public class Chain {
        
  }*/
}
