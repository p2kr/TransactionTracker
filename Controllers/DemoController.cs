using Microsoft.AspNetCore.Mvc;
using TransactionTracker.Utils;
using System.Net.Http;

namespace TransactionTracker.Controllers
{
    [ApiController]
    [Route("/demo")]
    public class DemoController : Controller
    {
        [HttpGet(Name = "Demo")]
        public IActionResult Demo()
        {
            try
            {
                var config = new ConnectorConfig();
                var emails = GmailConnector2.ConnectToGmail(config);
                emails.Wait();
                return Json(emails.Result);
            }
            catch (Exception ex)
            {
                return ErrorHandlers.ErrorResult(ex);
            }
        }
    }
}
