namespace TransactionTracker.src.Models
{
	public class Vendor : AuditedEntity
	{
		public string? Name { get; set; }
		public string? SearchQuery { get; set; }
		public VendorCategory? Category { get; set; }
		public ISet<VendorProperties>? FkVendorProperties { get; set; }
	}
}
