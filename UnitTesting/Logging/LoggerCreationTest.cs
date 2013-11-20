using ContainerAgent;
using LoggingFacility;
using LoggingFacility.Loggers;
using UnitTesting.Logging.UnitTesting;
using Xunit;

namespace UnitTesting.Logging {
    public class LoggerCreationTest {
        [Fact]
        public void CreateLogger() {
            var factory = Agent.Factory();
            var someLogga = factory.GetInstance<ILogger>();

            Assert.NotNull(someLogga);

            // not type is being initialized, hence
            Assert.Equal(someLogga.GetType(), typeof(NullLogger));

            //// todo put context into testSetup so that to llok how it works separately
            //// todo play with Global and Local thresholds
            //// todo try all loggers type separately and together
        }

        [Fact]
        public void CreateLoggerInClass() {
            var factory = Agent.Factory();
            var anX = factory.GetInstance<SomeAnnotatedLogga>();
            var context = factory.GetInstance<ILoggingContext>();

            Assert.NotNull(anX.Logger);
            Assert.Equal(anX.Logger.Name, typeof(SomeAnnotatedLogga).ToString());
            Assert.Equal(anX.Logger.Threshold, context.GlobalThreshold);
        }

        [Fact]
        public void LocalVsGlobalThresholds() {
            var factory = Agent.Factory();
            
            var anX1 = factory.GetInstance<SomeAnnotatedLogga>();
            var anX2 = factory.GetInstance<SomeAnnotatedLogga>();

            var context = factory.GetInstance<ILoggingContext>();
            var context1 = factory.GetInstance<ILoggingContext>();

            Assert.True(ReferenceEquals(context, context1)); // checking that context is singleton

            Assert.False(ReferenceEquals(anX1, anX2)); // checking that these are different

            Assert.NotNull(anX1.Logger);
            Assert.NotNull(anX2.Logger);

            Assert.False(ReferenceEquals(anX1.Logger, anX2.Logger));

            Assert.Equal(anX1.Logger.Name, typeof(SomeAnnotatedLogga).ToString());
            Assert.Equal(anX2.Logger.Name, typeof(SomeAnnotatedLogga).ToString());

            Assert.Equal(anX1.Logger.Threshold, context.GlobalThreshold);
            Assert.Equal(anX2.Logger.Threshold, context.GlobalThreshold);

            Assert.NotEqual(context.GlobalThreshold, Level.Warn);
            context.GlobalThreshold = Level.Warn;
            Assert.Equal(context.GlobalThreshold, Level.Warn);

            Assert.Equal(anX1.Logger.Threshold, context.GlobalThreshold);
            Assert.Equal(anX2.Logger.Threshold, context.GlobalThreshold);

            anX1.Logger.Threshold = Level.Debug;
            Assert.NotEqual(anX1.Logger.Threshold, context.GlobalThreshold);
            Assert.Equal(anX2.Logger.Threshold, context.GlobalThreshold);

            context.GlobalThreshold = Level.Warn;
            Assert.Equal(context.GlobalThreshold, Level.Warn);

            Assert.Equal(anX1.Logger.Threshold, context.GlobalThreshold);
            Assert.Equal(anX2.Logger.Threshold, context.GlobalThreshold);

        }
    }
}
