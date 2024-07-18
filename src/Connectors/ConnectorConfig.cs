namespace TransactionTracker.src.Connectors
{
	public class ConnectorConfig
	{
		// File location configs
		public string CredentialsDatastore { get; set; } = "Assets/Emails.UserInbox";

		public string ClientSecretsLocation { get; set; } = "Assets/client_secrets.json";

		// Message request configs
		public int MaxResults { get; set; } = 10;
		public bool IncludeSpamTrash { get; set; } = true;
		public string Labels { get; set; } = "INBOX";
		public string? Query { get; set; }
	}
}