using Serilog;

namespace TransactionTracker.Utils
{
	public static class CustomLogger
	{
		public static void ConfigureLogger()
		{
			Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Information()
			.WriteTo.Console()
			.WriteTo.File("logs/logs.log", rollingInterval: RollingInterval.Day)
			.CreateLogger();
		}
	}
}
