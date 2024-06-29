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
                var emails = GmailConnector2.ConnectToGmail();
                emails.Wait();
                return Json(emails.Result);
            }
            catch (Exception ex)
            {
                //return Problem(title: ex.Message, detail: ex.StackTrace);
                return ErrorHandlers.ErrorResult(ex);
            }
        }
    }
}
