namespace DataProvider.Loaders.Realtime {
    public interface IRealtime {
    
        IRealtime WithFeed(string feed);
        IRealtime WithRics(params string[] rics);
        ISubscriptionSetup Subscribe();
    }
}