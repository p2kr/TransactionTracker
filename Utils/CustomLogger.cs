namespace TransactionTracker.Utils
{
    public static class CustomLogger
    {

        private static readonly ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        public static ILogger<T> GetLogger<T>()
        {
            return loggerFactory.CreateLogger<T>();
        }

        public static ILogger GetLogger(Type type)
        {
            return loggerFactory.CreateLogger(type);
        }
    }
}
