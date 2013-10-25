using Connect;
using DataProvider.DataLoaders;
using DataProvider.DataLoaders.Status;
using DataProvider.RawData;
using LoggingFacility;
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

            c.AddRegistry<LoggerRegistry>();

            //----------------- Eikon Connection
            c.For<IConnection>().Singleton().Use<Connection>(); // singleton

            //----------------- Eikon Objects
            c.For<IEikonObjects>().Use<EikonObjectsPlVba>().Named("plvba"); // options available
            c.For<IEikonObjects>().Use<EikonObjectsPlain>().Named("plain");
            c.For<IEikonObjects>().Use<EikonObjectsSdk>().Named("sdk");     // last (i.e. default) option
                
            //---------------- Data Providers
            c.For<IRealtime>().Use<AdfinRealtime>();
        }
    }
}
