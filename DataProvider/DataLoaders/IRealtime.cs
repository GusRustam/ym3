namespace DataProvider.DataLoaders {
    public interface IRealtime {
        IRealtime WithFeed(string feed);
        IRealtime WithRics(params string[] rics);
        ISubscriptionSetup Subscribe();
    }
}