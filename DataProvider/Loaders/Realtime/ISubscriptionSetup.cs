using System;

namespace DataProvider.Loaders.Realtime {
    public interface ISubscriptionSetup {
        ISubscriptionSetup WithFrq(TimeSpan span);
        ISubscriptionSetup WithMode(string mode);
        ISubscriptionSetup WithFields(params string[] fields);

        ISnapshotTicker SnapshotReuqest(TimeSpan? timeout = null);
        IFieldsTicker FieldsRequest(TimeSpan? timeout = null);

        ISubscription DataRequest();
    }
}