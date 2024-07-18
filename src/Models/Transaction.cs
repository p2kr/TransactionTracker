namespace TransactionTracker.src.Models
{
	public class Transaction : AuditedEntity
	{
		public string? TransactionNumber { get; set; }
		public string Description { get; set; } = "## NA ##";
		public double Credited { get; set; }
		public double Debited { get; set; }
		public Vendor? Vendor { get; set; }
		public DateTime? TransactionTime { get; set; }
		public string Currency { get; set; } = "INR";
	}
}
