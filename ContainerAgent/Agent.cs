using Connect;
using DataProvider.Loaders.Chain;
using DataProvider.Loaders.Chain.Data;
using DataProvider.Loaders.History;
using DataProvider.Loaders.History.Data;
using DataProvider.Loaders.Metadata;
using DataProvider.Loaders.Realtime;
using DataProvider.Objects;
using DataProvider.Storage;
using LoggingFacility;
using LoggingFacility.Loggers;
using StructureMap;
using StructureMap.Configuration.DSL.Expressions;

namespace ContainerAgent {
    public class Agent {
        private static Container _container;

        private Agent() {
        }

        public static IContainer Factory(AgentMode mode = null) {
            return _container ?? (_container = new Container(Configure));
        }
         
        private static void Configure(ConfigurationExpression c) {
            //----------------- Loggging
            c.For<ILoggingContext>().Singleton().Use<LoggingContext>(); // singleton

            c.For<ILogger>().AlwaysUnique().Use(context => {
                var tp = context.ParentType ?? context.BuildStack.Current.ConcreteType;
                if (tp == null)
                    return new NullLogger();

                var loggingContext = context.GetInstance<ILoggingContext>();
                var name = tp.Name;

                // todo return Logging attribute with default name and threshold level
                var res = new NLogLogger(loggingContext.GlobalThreshold, name);
                loggingContext.RegisterLogger(res);

                return res;
            });

            //c.For<ILogger>().ConditionallyUse(x => x.If(c => c.)) // todo!! wut

            //----------------- Eikon Connection
            c.For<IConnection>().Singleton().Use<Connection>(); // singleton

            //---------------- Realtime data 
            c.For<IRealtime>().Use<AdfinRealtime>();

            c.For<ISubscriptionSetup>().Use<AdfinSubscriptionSetup>();
            c.For<ISnapshotTicker>().Use<AdfinSnapshotTicker>();
            c.For<IFieldsTicker>().Use<AdfinFieldsTicker>();
            c.For<ISubscription>().Use<AdfinSubscription>();
            c.For<ISubscriptionSetup>().Use<AdfinSubscriptionSetup>();

            //---------------- Historical data
            c.For<IHistory>().Use<History>();
            c.For<IHistoryRequest>().Use<MultiHistoryRequest>().Named("multi");
            c.For<IHistoryRequest>().Use<AdxHistoryRequest>().Named("single");

            //---------------- History container
            c.For<IHistoryContainer>().Use<HistoryContainer>();
            c.For(typeof(IStorage<,,,>)).Use(typeof(SparseStorage<,,,>));
            c.For(typeof(IStorage<,,>)).Use(typeof(SparseStorage<,,>));

            //----------------- Chain data
            c.For<IChainResponse>().Use<ChainResponse>();

            //----------------- Chain loader
            c.For<IChain>().Use<Chain>();
            c.For<IChainRequest>().Use<MultiChainRequest>().Named("multi");
            c.For<IChainRequest>().Use<SingleChainRequest>().Named("single");

            //----------------- Dex2
            c.For(typeof(IMetadata<>)).Use(typeof(Metadata<>));
            c.For(typeof(IMetaObjectFactory<>)).Use(typeof(MetaObjectFactory<>));
            c.For(typeof(IMetadataRequest<>)).Use(typeof(MetadataRequest<>));
            c.For(typeof(IRequestSetup<>)).Use(typeof(RequestSetup<>));
            c.For(typeof(IMetadataContainer<>)).Use(typeof(MetadataContainer<>));
            c.For(typeof(IMetadataExporter<>)).Use(typeof(MetadataExporter<>));

            //----------------- Eikon Objects
            c.For<IEikonObjects>().Use<EikonObjectsSdk>();

            // Profiles
            c.Profile(AgentMode.InEikon, InEikonProfile);
            c.Profile(AgentMode.External, ExternalProfile);
        }

        private static void ExternalProfile(ProfileExpression p) {
            //----------------- Eikon Objects
            p.For<IEikonObjects>().Use<EikonObjectsSdk>();
        }

        private static void InEikonProfile(ProfileExpression p) {
            p.For<IEikonObjects>().Use<EikonObjectsPlain>();
        }
    }
}
