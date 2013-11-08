using System;
using StructureMap;

namespace DataProvider.Loaders.Chain {
    public class Chain : IChain {
        private readonly IContainer _container;
        private readonly ChainSetup _setup = new ChainSetup();

        public Chain(IContainer container) {
            _container = container;
        }

        public Chain(IContainer container, ChainSetup prms)
            : this(container) {
            _setup = prms;
        }

        public IChain WithFeed(string feed) {
            var prms = _setup.Clone();
            prms.Feed = feed;
            return _container
                .With(prms)
                .GetInstance<IChain>();
        }

        public IChain WIthChain(Action<IChainData> callback) {
            var prms = _setup.Clone();
            prms.Callback = callback;
            return _container
                .With(prms)
                .GetInstance<IChain>();
        }

        public IChain WithRics(params string[] rics) {
            var prms = _setup.Clone();
            
            foreach (var ric in rics) 
                prms.Rics.Add(ric);
            
            return _container
                .With(prms)
                .GetInstance<IChain>();
        }

        public IChain SkipRange(int @from, int to) {
            var prms = _setup.Clone();
            prms.AddMode(@from, to);
            return _container
                .With(prms)
                .GetInstance<IChain>();
        }

        public IChain SkipItem(int num) {
            var prms = _setup.Clone();
            prms.AddMode(num);
            return _container
                .With(prms)
                .GetInstance<IChain>();
        }

        public IChainRequest Subscribe() {
            _setup.Validate();
            return _container
                .With(_setup)
                .GetInstance<IChainRequest>(_setup.Rics.Count == 1 ? "single" : "multi");
        }
    }
}