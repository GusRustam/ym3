using System;
using System.Collections.Generic;
using System.Linq;
using DataProvider.Loaders.History.Data;
using Toolbox;

namespace DataProvider.Loaders.History {
    public struct HistorySetup {
        public string Feed;
        public DateTime? Since;
        public DateTime? Till;
        public int? Rows;
        public Action<IHistoryContainer> Callback;
        public ICollection<IHistoryField> Fields;

        public void Validate() {
            if (string.IsNullOrEmpty(Feed))
                throw new ArgumentException("feed");

            if (!Fields.ToSomeArray().Any())
                throw new ArgumentException("no fields");

            var cN = 0;
            cN += Since.HasValue ? 0 : 1;
            cN += Till.HasValue ? 0 : 1;
            cN += Rows.HasValue ? 0 : 1;

            if (cN == 0)
                throw new ArgumentException("since, till and nbrows together");
            if (cN >= 3)
                throw new ArgumentException("insufficient since, till and nbrows data");

            if (!Feed.Any())
                throw new ArgumentException("No fields");
        }
    }
}