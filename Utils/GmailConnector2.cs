using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Util.Store;
using System.Text;

namespace TransactionTracker.Utils
{
    public static class GmailConnector2
    {
        private static readonly ILogger LOG = CustomLogger.GetLogger(typeof(GmailConnector2));

        private static readonly string CLIENT_SECRETS = "Assets/client_secrets.json";
        public static async Task<List<string>> ConnectToGmail()
        {
            List<string> listOfMessages = [];

            UserCredential credential;
            using (var stream = new FileStream(CLIENT_SECRETS, FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    (await GoogleClientSecrets.FromStreamAsync(stream)).Secrets,
                    [GmailService.Scope.GmailReadonly],
                    "user",
                    CancellationToken.None,
                    new FileDataStore("Assets/Emails.UserInbox")
                    );
            }

            // Create the service
            var service = new GmailService(new GmailService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "TransationTracker",
            });

            var inboxListRequest = service.Users.Messages.List("me");
            inboxListRequest.LabelIds = "INBOX";
            inboxListRequest.IncludeSpamTrash = true;

            var msgList = await inboxListRequest.ExecuteAsync();

            if (msgList != null && msgList.Messages != null)
            {
                foreach (var msg in msgList.Messages)
                {
                    try
                    {

                        var fullMessageRequest = service.Users.Messages.Get("me", msg.Id);
                        var fullMessageResponse = await fullMessageRequest.ExecuteAsync();
                        var msgParts = fullMessageResponse.Payload;

                        StringBuilder message = new("");

                        if (msgParts.Parts != null && msgParts.Parts.Any())
                        {
                            foreach (var part in msgParts.Parts)
                            {
                                if (part?.Body?.Data != null)
                                {
                                    message.Append(Encoding.UTF8.GetString(FromBase64ForUrlString(part.Body.Data)));
                                }
                            }
                        }
                        else if (msgParts.Body != null && msgParts.Body.Data.Length != 0)
                        {
                            message.Append(Encoding.UTF8.GetString(FromBase64ForUrlString(msgParts.Body.Data)));
                        }

                        listOfMessages.Add(message.ToString());
                    }
                    catch (Exception ex)
                    {
                        LOG.LogError(ex, "Error in {Snippet}", msg.Snippet);
                    }
                }
            }
            return listOfMessages;
        }
        public static byte[] FromBase64ForUrlString(string base64ForUrlInput)
        {
            int padChars = (base64ForUrlInput.Length % 4) == 0 ? 0 : (4 - (base64ForUrlInput.Length % 4));
            StringBuilder result = new(base64ForUrlInput, base64ForUrlInput.Length + padChars);
            result.Append(String.Empty.PadRight(padChars, '='));
            result.Replace('-', '+');
            result.Replace('_', '/');
            return Convert.FromBase64String(result.ToString());
        }
    }
}
