namespace DataProvider.Loaders.Realtime {
    /*
     * todo huge refactoring
     * 1) 3 folders - 
     *      - Streaming
     *      - Snapshot
     *      - Fields
     * 2) In common folder - general fluent API to create loaders
     * 3) Snapshot and Fields - via Toolbox.Async
     * 4) Use factory (see Metadata)
     * 5) Ability to have Streaming either as singleton (so that to load data synchronously) or transient
     * 6) In case of singleton have an ability to add and remove rics on the fly.
     *    note question is what would happen to fields, will it start loading ALL FIELDS for all rics?
     * 7) In case of transient keep interface generally same to current one
     */
    public interface IRealtime {
        IRealtime WithFeed(string feed);
        IRealtime WithRics(params string[] rics);
        ISubscriptionSetup Subscribe();
    }
}