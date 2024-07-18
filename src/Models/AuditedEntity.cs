namespace TransactionTracker.src.Models
{
	public abstract class AuditedEntity
	{
		public int Id { get; set; }
		public DateTime CreateTime { get; set; } = DateTime.Now;
		public DateTime UpdateTime { get; set; } = DateTime.Now;
		public string CreateUser { get; set; } = "System";
		public string UpdateUser { get; set; } = "System";
	}
}
