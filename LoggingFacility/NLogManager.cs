using System;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace LoggingFacility {
    public class NLogManager {
        public static readonly string LogFileName = string.Format("YieldMap_{0:ddMMyyyy}.log", DateTime.Today);
        public static readonly string LogFilePath = Path.GetTempPath();
        public static string ZipFileName = string.Format("attachments_{0:ddMMyyyy}.zip", DateTime.Today);

        // default settings
        public LogLevel LoggingLevel {
            get {
                return _loggingLevel;
            }
            internal set {
                _loggingLevel = value;
                LogManager.GlobalThreshold = LoggingLevel;
            }
        }

        public static NLogManager Instance {
            get {
                return _instance ?? (_instance = new NLogManager());
            }
        }

        private LogLevel _loggingLevel = LogLevel.Trace;
        private static NLogManager _instance;

        private NLogManager() {
            //' 1) Creating logger text config
            var txtTarget = new FileTarget {
                FileName = Path.Combine(LogFilePath, LogFileName),
                DeleteOldFileOnStartup = true,
                Name = "Main",
                Layout = Layout.FromString("${date} \t ${level} \t ${callsite:includeSourcePath=false} | ${message} | ${exception:format=Type,Message} | ${stacktrace}")
            };

            //' 2) Creating logger UDP config
            var udpTarget = new ChainsawTarget {
                Address = "udp://127.0.0.1:7071",
                Name = "Chainsaw",
                Layout = new Log4JXmlEventLayout()
            };

            var consoleTarget = new ConsoleTarget {
                Layout = Layout.FromString("${date} \t ${level} \t ${logger} \t ${message}")
            };

            //' 4) Selecting congiguration and initializing
            var logger = LogManager.GetCurrentClassLogger();
            var loggerConfig = new LoggingConfiguration();
            loggerConfig.LoggingRules.Add(new LoggingRule("*", LoggingLevel, consoleTarget));
            loggerConfig.LoggingRules.Add(new LoggingRule("*", LoggingLevel, txtTarget));
            loggerConfig.LoggingRules.Add(new LoggingRule("*", LoggingLevel, udpTarget));
            LogManager.Configuration = loggerConfig;

            logger.Debug("Logger initialized");
        }

        public Logger GetLogger(Type type) {
            return LogManager.GetLogger(type.Name);
        }

        public Logger GetLogger(string name) {
            return LogManager.GetLogger(name);
        }
    }
}