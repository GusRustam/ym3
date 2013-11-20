using System;
using LoggingFacility;
using LoggingFacility.Loggers;
using LoggingFacility.LoggingSupport;
using Moq;
using UnitTesting.Logging.UnitTesting;
using Xunit;
using Xunit.Extensions;

namespace UnitTesting.Logging {
    public class LoggerUnitTest {
        [Fact]
        public void MessageFiltersOut() {
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.Log(Level.Info, "Hello world", null));

            var myObj = new SomeLogga(mockLogger.Object);
            myObj.Greet();

            mockLogger.Verify(x => x.Log(It.IsAny<Level>(), It.IsAny<string>(), null), Times.Never);
        }

        [Fact]
        public void MessagePasses() {
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.Threshold).Returns(Level.Trace);
            mockLogger.Setup(x => x.Log(Level.Info, "Hello world", null));

            var myObj = new SomeLogga(mockLogger.Object);
            myObj.Greet();

            mockLogger.Verify(x => x.Log(It.IsAny<Level>(), It.IsAny<string>(), null), Times.Once);
        }

        [Fact]
        public void TryNullLogger() {
            var x = new NullLogger();
            x.Log(Level.Trace, "Hello Null");
            x.Log(Level.Info, "Hello Null 2", new Exception("Wow"));

            // ReSharper disable NotAccessedVariable
            // ReSharper disable RedundantAssignment
            var t = x.Threshold;
            var p = x.Name;

            t = Level.Off;
            p = "Babushka";
            // ReSharper restore RedundantAssignment
            // ReSharper restore NotAccessedVariable
        }

        [Theory]
        [ClassData(typeof (WrappedLoggers))]
        public void DistinctLoggers(LoggerWrapper w) {
            var messages = 0;
            var exceptions = 0;
            w.Logged += (level, s, ex) => {
                messages = messages + 1;
                if (ex != null)
                    exceptions = exceptions + 1;
            };

            var x = new SomeLogga(w) { Logger = { Threshold = Level.Debug }};

            x.Trace("Hello Logger");
            Assert.Equal(messages, 0);
            Assert.Equal(exceptions, 0);

            x.Info("Hello Logger 2", new Exception("Wow"));
            Assert.Equal(messages, 1);
            Assert.Equal(exceptions, 1);

            x.Error("Hello Logger 3");
            Assert.Equal(messages, 2);
            Assert.Equal(exceptions, 1);
            
            x.Logger.Threshold = Level.Off; // cutting off all logging
            x.Error("Hello Logger 4");
            Assert.Equal(messages, 2);
            Assert.Equal(exceptions, 1);
        }

        [Fact]
        public void TrySupportsLogger() {
            var s = new Mock<ISupportsLogging>();

            var calls = 0;
            s.Setup(e => e.Logger.Log(It.IsAny<Level>(), It.IsAny<string>(), It.IsAny<Exception>())).Callback(() => { calls++; });
            s.SetupGet(e => e.Logger.Threshold).Returns(Level.Info);

            var obj = s.Object;
            var n = obj.Logger.Name;    
            Assert.Null(n);

            obj.Info("Wow");
            Assert.Equal(calls, 1);
        }
    }
}
