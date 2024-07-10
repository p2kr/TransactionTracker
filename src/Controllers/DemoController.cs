using Microsoft.AspNetCore.Mvc;
using Serilog;
using TransactionTracker.src.Connectors;
using TransactionTracker.src.Utils;

namespace TransactionTracker.src.Controllers
{
	[ApiController]
	[Route("/demo")]
	public class DemoController : Controller
	{
		[HttpGet("ViewLatest10MessagesAsJson")]
		public IActionResult ViewLatest10MessagesAsJson()
		{
			try
			{
				var config = new ConnectorConfig();
				var emails = GmailConnector.ConnectToGmail(config);
				emails.Wait();
				return Json(emails.Result);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error in Demo: ");
				return ErrorHandlers.ErrorResult(ex);
			}
		}

		[HttpGet("ViewLatestMessageAsHtml")]
		public IActionResult ViewLatestMessageAsHtml()
		{
			try
			{
				var config = new ConnectorConfig
				{
					MaxResults = 1
				};
				var emails = GmailConnector.ConnectToGmail(config);
				emails.Wait();
				return Content(emails.Result[0], "text/html");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error in ViewLatestMessageAsHtml: ");
				return ErrorHandlers.ErrorResult(ex);
			}
		}
	}
}
