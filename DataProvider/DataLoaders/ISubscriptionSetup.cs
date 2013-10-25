using System;

namespace DataProvider.DataLoaders {
    public interface ISubscriptionSetup {
        ISubscriptionSetup WithFrq(TimeSpan span);
        ISubscriptionSetup WithFields(params string[] fields);

        ISnapshotTicker ReuqestSnapshot(TimeSpan? timeout = null);
        IFieldsTicker RequestFields(TimeSpan? timeout = null);

        //request fields
        //string[] RequestFields();
        /* 2b implemented
         * ISubscriptionSetup OnSourceStatus(string sourceName, DataSourceStatus status);
         * ISubscriptionSetup OnSnapshot(IDataTable data);
         * ISubscriptionSetup OnUpdate(IDataTable data);
         */
        ISubscription Create();
    }
}