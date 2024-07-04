using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Util.Store;
using System.Diagnostics;
using System.Text;

namespace TransactionTracker.Utils
{
	public static class GmailConnector2
	{
		private static readonly ILogger Log = CustomLogger.GetLogger(typeof(GmailConnector2));

		/// <summary>
		/// Connect to Gmail service and fetch emails
		/// </summary>
		/// <returns>List of email text in html format</returns>
		public static async Task<List<string>> ConnectToGmail(ConnectorConfig config)
		{
			var watch = Stopwatch.StartNew();
			List<string> listOfMessages = [];

			UserCredential credential = await GetCredential(config);

			// Create the service
			var service = new GmailService(new GmailService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = "TransationTracker",
			});

			var inboxListRequest = service.Users.Messages.List("me");
			BuildMessagesRequest(inboxListRequest, config);

			var msgList = await inboxListRequest.ExecuteAsync();

			if (msgList != null && msgList.Messages != null)
			{
				foreach (var msg in msgList.Messages)
				{
					if (msg != null)
					{
						await GetAllMessages(listOfMessages, service, msg);
					}
				}
				Log.LogInformation("Found {N} messages in {Timespan}", listOfMessages.Count, watch.Elapsed);
			}
			return listOfMessages;
		}

		private static async Task GetAllMessages(List<string> listOfMessages, GmailService service, Message msg)
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
				Log.LogError(ex, "Error in {Snippet}", msg.Snippet);
			}
		}

		private static async Task<UserCredential> GetCredential(ConnectorConfig config)
		{
			UserCredential credential;
			using (var stream = new FileStream(config.ClientSecretsLocation, FileMode.Open, FileAccess.Read))
			{
				credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
					(await GoogleClientSecrets.FromStreamAsync(stream)).Secrets,
					[GmailService.Scope.GmailReadonly],
					"user",
					CancellationToken.None,
					new FileDataStore(config.CredentialsDatastore)
					);
			}
			return credential;
		}

		private static void BuildMessagesRequest(UsersResource.MessagesResource.ListRequest msgRequest, ConnectorConfig config)
		{
			msgRequest.LabelIds = config.Labels;
			msgRequest.IncludeSpamTrash = config.IncludeSpamTrash;
			msgRequest.MaxResults = config.MaxResults;
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
