using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Util.Store;
using System.Text;

namespace TransactionTracker.Utils
{
    public class GmailConnector
    {
        private static readonly string CLIENT_SECRETS = "Assets/client_secrets.json";
        private static readonly string MY_EMAIL_ID = "prince2000kr@gmail.com";
        public static async Task<List<string>> ConnectToGmail()
        {
            List<string> messages = [];
            UserCredential credential;
            using (var stream = new FileStream(CLIENT_SECRETS, FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    [GmailService.Scope.GmailReadonly],
                    "user",
                    CancellationToken.None,
                    new FileDataStore("Emails.UserInbox")
                    );
            }

            // Create the service
            var service = new GmailService(new GmailService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "TransationTracker",
            });

            var inboxlistRequest = service.Users.Messages.List(MY_EMAIL_ID);
            inboxlistRequest.LabelIds = "INBOX";
            inboxlistRequest.IncludeSpamTrash = true;

            var emailListResponse = inboxlistRequest.Execute();
            if (emailListResponse != null && emailListResponse.Messages != null)
            {
                //loop through each email and get what fields you want...   
                foreach (var email in emailListResponse.Messages)
                {
                    var emailInfoRequest = service.Users.Messages.Get(MY_EMAIL_ID, email.Id);
                    var emailInfoResponse = emailInfoRequest.Execute();
                    if (emailInfoResponse != null)
                    {
                        String from = "";
                        String date = "";
                        String subject = "";
                        //loop through the headers to get from,date,subject, body  
                        foreach (var mParts in emailInfoResponse.Payload.Headers)
                        {
                            if (mParts.Name == "Date")
                            {
                                date = mParts.Value;
                            }
                            else if (mParts.Name == "From")
                            {
                                from = mParts.Value;
                            }
                            else if (mParts.Name == "Subject")
                            {
                                subject = mParts.Value;
                            }
                            if (date != "" && from != "")
                            {
                                foreach (MessagePart p in emailInfoResponse.Payload.Parts)
                                {
                                    if (p.MimeType == "text/html")
                                    {
                                        byte[] data = FromBase64ForUrlString(p.Body.Data);
                                        string decodedString = Encoding.UTF8.GetString(data);
                                        messages.Add(decodedString);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return messages;
        }
        public static byte[] FromBase64ForUrlString(string base64ForUrlInput)
        {
            int padChars = (base64ForUrlInput.Length % 4) == 0 ? 0 : (4 - (base64ForUrlInput.Length % 4));
            StringBuilder result = new StringBuilder(base64ForUrlInput, base64ForUrlInput.Length + padChars);
            result.Append(String.Empty.PadRight(padChars, '='));
            result.Replace('-', '+');
            result.Replace('_', '/');
            return Convert.FromBase64String(result.ToString());
        }
    }
}
