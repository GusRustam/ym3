using Connect;
using DataProvider.Loaders.History;
using DataProvider.Loaders.History.Data;
using DataProvider.Loaders.Realtime;
using DataProvider.Objects;
using LoggingFacility;
using LoggingFacility.Loggers;
using StructureMap;

namespace ContainerAgent {
    public class Agent {
        private static Container _container;

        private Agent() {
        }

        public static IContainer Factory() {
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
                var name = tp.ToString();
                var res = new NLogLogger(loggingContext.GlobalThreshold, name);
                loggingContext.RegisterLogger(res);

                return res;
            });

            // todo!! wut
            //c.For<ILogger>().ConditionallyUse(x => x.If(c => c.))

            //----------------- Eikon Connection
            c.For<IConnection>().Singleton().Use<Connection>(); // singleton

            //----------------- Eikon Objects
            c.For<IEikonObjects>().Use<EikonObjectsPlVba>().Named("plvba"); // options available
            c.For<IEikonObjects>().Use<EikonObjectsPlain>().Named("plain");
            c.For<IEikonObjects>().Use<EikonObjectsSdk>().Named("sdk");     // last (i.e. default) option
                
            //---------------- Realtime data 
            c.For<IRealtime>().Use<AdfinRealtime>();

            c.For<ISubscriptionSetup>().Use<AdfinSubscriptionSetup>();
            c.For<ISnapshotTicker>().Use<AdfinSnapshotTicker>();
            c.For<IFieldsTicker>().Use<AdfinFieldsTicker>();
            c.For<ISubscription>().Use<AdfinSubscription>();
            c.For<ISubscriptionSetup>().Use<AdfinSubscriptionSetup>();

            // Adfin RT
            //c.For<AdxRtList>().Use(context => context.GetInstance<IEikonObjects>().CreateAdxRtList());

            //---------------- Historical data
            c.For<IHistory>().Use<History>();
            c.For<IHistoryRequest>().Use<TsiHistoryRequest>().Named("tsi6");
            c.For<IHistoryRequest>().Use<AdxHistoryRequest>().Named("tsi1"); // last, i.e. default

            //---------------- History container
            c.For<IHistoryContainer>().Use<HistoryContainer>();
            c.For(typeof(IStorage<,,,>)).Use(typeof(SparseStorage<,,,>));
            c.For(typeof(IStorage<,,>)).Use(typeof(SparseStorage<,,>));
        }
    }
}
