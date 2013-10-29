using System;
using DataProvider.Loaders.History.Data;

namespace DataProvider.Loaders.History {
    public interface IHistoryRequest {
        /// <summary>Set feed to retrieve data. Default feed is IDN</summary>
        /// <param name="feed">Feed name</param>
        IHistoryRequest WithFeed(string feed);

        /// <summary>Set data interval. Default interval is Day</summary>
        /// <param name="interval">Interval type</param>
        IHistoryRequest WithInterval(HistoryInterval interval);

        /// <summary>Set date since which history will be requested</summary>
        /// <param name="date">Interval type</param>
        /// <remarks>You can't set <see cref="WithSince"/>, <see cref="WithTill"/> and <see cref="WithNumRecords"/> together</remarks>
        IHistoryRequest WithSince(DateTime date);

        /// <summary>Set date till which history will be requested</summary>
        /// <param name="date">Interval type</param>
        /// <remarks>You can't set <see cref="WithSince"/>, <see cref="WithTill"/> and <see cref="WithNumRecords"/> together</remarks>
        IHistoryRequest WithTill(DateTime date);

        /// <summary>Set number of historical rows to be requested</summary>
        /// <param name="num">Number of rows</param>
        /// <remarks>You can't set <see cref="WithSince"/>, <see cref="WithTill"/> and <see cref="WithNumRecords"/> together</remarks>
        IHistoryRequest WithNumRecords(int num);

        /// <summary>Set data callback</summary>
        /// <param name="callback">Interval type</param>
        IHistoryRequest WithCallback(Action<IHistorySnapshot> callback);

        /// <summary>Set mode - Time/Sales or Time/Series. Default is Time/Sales</summary>
        /// <param name="mode">Mode</param>
        IHistoryRequest WithMode(HistoryMode mode);

        /// <summary>Request the data</summary>
        void Request();
    }
}