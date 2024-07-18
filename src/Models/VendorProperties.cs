namespace TransactionTracker.src.Models
{
	public class VendorProperties : AuditedEntity
	{
		public Vendor? FkVendors { get; set; }
		public string? Config { get; set; }
		public VendorPropertiesConfigType VendorPropertiesConfigType { get; set; }
		public int? Groups { get; set; }
	}

	public enum VendorPropertiesConfigType
	{
		CSS_SELECTOR,
		REGEX
	}
}
