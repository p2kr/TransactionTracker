using Serilog;

namespace TransactionTracker.src.Utils
{
    public static class CustomLogger
    {
        public static void ConfigureLogger()
        {
            if ("true".Equals(Environment.GetEnvironmentVariable("IsDevelopment")))
            {
                Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("logs/logs.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            }
            else
            {
                //TODO: Make it more customizables
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.File("logs/logs.log", rollingInterval: RollingInterval.Day)
                    .CreateLogger();
            }
        }
    }
}
