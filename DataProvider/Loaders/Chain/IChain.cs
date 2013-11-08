using System;

namespace DataProvider.Loaders.Chain {
    public interface IChain {
        /// <summary> Set feed </summary>
        /// <param name="feed">Feed name</param>
        IChain WithFeed(string feed);

        /// <summary> Set data callback </summary>
        /// <param name="callback">Function to call when data received</param>
        IChain WIthChain(Action<IChainData> callback);

        /// <summary>
        /// Set chain rics to load
        /// </summary>
        /// <param name="rics"></param>
        /// <returns></returns>
        IChain WithRics(params string[] rics);

        IChain SkipRange(int @from, int to);

        IChain SkipItem(int num);

        /// <summary> Load chains </summary>
        IChainRequest Subscribe();
    }
}