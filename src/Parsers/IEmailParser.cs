using TransactionTracker.src.Models;

namespace TransactionTracker.src.Parsers
{
	public interface IEmailParser
	{
		public Transaction Parse(string emailBody, Vendor vendor);
	}
}
