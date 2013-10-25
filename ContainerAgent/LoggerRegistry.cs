using LoggingFacility;
using LoggingFacility.Loggers;
using StructureMap;
using StructureMap.Configuration.DSL;

namespace ContainerAgent {
    internal class LoggerRegistry : Registry {
        public LoggerRegistry() {
            For<ILogger>().Use(ResolveLogger);
        }

        private static ILogger ResolveLogger(IContext context) {
            var tp = context.ParentType;
            if (tp == null)
                return new NullLogger();

            var loggingContext = context.GetInstance<ILoggingContext>();
            var name = tp.ToString();
            var res = new NLogLogger(loggingContext.GlobalThreshold, name);
            loggingContext.RegisterLogger(res);

            return res;
        }
    }
}