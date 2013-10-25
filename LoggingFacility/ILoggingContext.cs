namespace LoggingFacility {
    public interface ILoggingContext {
        Level GlobalThreshold { get; set; }
        void RegisterLogger(ILogger logger);
    }
}