using System;
using System.Collections.Generic;
using DataProvider.Loaders.History.Data;

namespace DataProvider.Loaders.History {
    public interface IHistory  {
        /// <summary>Set feed to retrieve data. Default feed is IDN</summary>
        /// <param name="feed">Feed name</param>
        IHistory WithFeed(string feed);

        /// <summary>Set date since which history will be requested</summary>
        /// <param name="date">Interval type</param>
        /// <remarks>You can't set <see cref="WithSince"/>, <see cref="WithTill"/> and <see cref="WithNumRecords"/> together</remarks>
        IHistory WithSince(DateTime date);

        /// <summary>Set date till which history will be requested</summary>
        /// <param name="date">Interval type</param>
        /// <remarks>You can't set <see cref="WithSince"/>, <see cref="WithTill"/> and <see cref="WithNumRecords"/> together</remarks>
        IHistory WithTill(DateTime date);

        /// <summary>Set number of historical rows to be requested</summary>
        /// <param name="num">Number of rows</param>
        /// <remarks>You can't set <see cref="WithSince"/>, <see cref="WithTill"/> and <see cref="WithNumRecords"/> together</remarks>
        IHistory WithNumRecords(int num);

        /// <summary>Set data callback</summary>
        /// <param name="callback">Interval type</param>
        IHistory WithHistory(Action<IHistoryContainer> callback);

        /// <summary>Append field to request</summary>
        /// <param name="field">field</param>
        IHistory AppendField(IHistoryField field);

        IHistory AppendFields(IEnumerable<IHistoryField> fields);

        IHistoryRequest Subscribe(string ric); // it's a nice way to parametrize constructor
    }
}
