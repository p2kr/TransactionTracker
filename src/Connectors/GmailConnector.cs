using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;
using System.Diagnostics;
using System.Text;

namespace TransactionTracker.src.Connectors
{
	public static class GmailConnector
	{
		/// <summary>
		/// Connect to Gmail service and fetch emails
		/// </summary>
		/// <returns>List of emails text in html format</returns>
		public static async Task<List<string>> ConnectToGmail(ConnectorConfig config)
		{
			var watch = Stopwatch.StartNew();
			List<string> listOfMessages = [];

			UserCredential credential = await GetCredential(config);

			// Create the service
			var service = new GmailService(new Google.Apis.Services.BaseClientService.Initializer()
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
						string? fullMsg = await GetFullMessage(service, msg);
						if (fullMsg != null)
						{
							listOfMessages.Add(fullMsg);
						}
					}
				}
				Log.Information("Found {N} messages in {Timespan}", listOfMessages.Count, watch.Elapsed);
			}
			return listOfMessages;
		}

		private static async Task<string?> GetFullMessage(GmailService service, Message msg)
		{
			try
			{
				var fullMessageRequest = service.Users.Messages.Get("me", msg.Id);
				if ("true".Equals(Environment.GetEnvironmentVariable("IsDevelopment")))
				{
					fullMessageRequest.PrettyPrint = true;
				}
				fullMessageRequest.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Full;
				var fullMessageResponse = await fullMessageRequest.ExecuteAsync();


				//var finalMsg = Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(fullMessageResponse.Raw));
				//return finalMsg;

				var msgParts = fullMessageResponse.Payload;
				StringBuilder message = new("");

				if (msgParts.MimeType.ToLower().StartsWith("multipart/mixed"))
				{
					foreach (var part in msgParts.Parts.Where(t => t.MimeType.ToLower().StartsWith("multipart/")))
					{
						foreach (var subPart in part.Parts)
						{
							if (subPart?.Body?.Data != null)
							{
								message.Append(Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(subPart.Body.Data)));
							}
						}

					}
				}
				else if (msgParts.MimeType.ToLower().StartsWith("multipart/"))
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

				return message.ToString();
			}
			catch (Exception ex)
			{

				Log.Error(ex, "Error in {Snippet}", msg.Snippet);
			}
			return null;
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

			if (!string.IsNullOrEmpty(config.Query))
			{
				msgRequest.Q = config.Query;
			}
		}

		public static byte[] FromBase64ForUrlString(string base64ForUrlInput)
		{
			int padChars = base64ForUrlInput.Length % 4 == 0 ? 0 : 4 - base64ForUrlInput.Length % 4;
			StringBuilder result = new(base64ForUrlInput, base64ForUrlInput.Length + padChars);
			result.Append(string.Empty.PadRight(padChars, '='));
			result.Replace('-', '+');
			result.Replace('_', '/');
			return Convert.FromBase64String(result.ToString());
		}
	}
}
