using Microsoft.AspNetCore.Mvc;
using Serilog;
using TransactionTracker.src.Connectors;
using TransactionTracker.src.Models;
using TransactionTracker.src.Parsers;
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

		[HttpGet("ParseOneMessageToDb")]
		public IActionResult ParseOneMessageToDb()
		{
			try
			{
				ConnectorConfig connectorConfig = new()
				{
					Query = "subject:Your Swiggy order was successfully delivered"
				};
				var emails = GmailConnector.ConnectToGmail(connectorConfig);
				emails.Wait();
				List<string> messages = emails.Result;
				if (messages.Count <= 0)
				{
					return Content("{'msg': 'No messages found'}", "application/json");
				}
				GenericEmailParserV1 emailParser = new();
				Vendor vendor = new()
				{
					Name = "Swiggy",
					Category = new()
					{
						Name = "Food"
					},
					FkVendorProperties = new HashSet<VendorProperties>()
				};
				vendor.FkVendorProperties.Add(new VendorProperties()
				{
					Config = "",//TODO: 
					VendorPropertiesConfigType = VendorPropertiesConfigType.CSS_SELECTOR,
					Groups = 2,
				});
				if (vendor.FkVendorProperties == null || vendor.FkVendorProperties.Count <= 0)
				{
					throw new NotSupportedException("Empty vendor properties");
				}
				var trn = emailParser.Parse(messages[0], vendor);
				return Json(trn);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error in ParseOneMessageToDb: ");
				return ErrorHandlers.ErrorResult(ex);
			}
		}
	}
}
