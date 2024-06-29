using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace TransactionTracker.Utils
{
    public static class ErrorHandlers
    {

        public static IActionResult ErrorResult(Exception ex)
        {
            return new JsonResult(new { message = ex.Message, stacktrace = ex.StackTrace })
            {
                StatusCode = 500,
                SerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore },
            };

        }
    }
}
