using HtmlAgilityPack;
using System.Globalization;
using System.Text.RegularExpressions;
using TransactionTracker.src.Models;

namespace TransactionTracker.src.Parsers
{
	public class GenericEmailParserV1 : IEmailParser
	{
		public Transaction Parse(string emailBody, Vendor vendor)
		{
			Transaction transaction = new();

			HtmlDocument doc = new();
			doc.LoadHtml(emailBody);

			HtmlNode transactionNumber = doc.DocumentNode.SelectSingleNode("//div[@class='order-id'][1]/table//td[1]/h5");
			if (transactionNumber != null)
			{
				transaction.TransactionNumber = transactionNumber.InnerText;
			}

			HtmlNode description = doc.DocumentNode.SelectSingleNode("//div[@class='order-id'][1]/table//td[2]/h5");
			if (description != null)
			{
				transaction.Description = description.InnerText;
			}

			//TODO: Use email received time instead
			HtmlNode transactionTime = doc.DocumentNode.SelectSingleNode("//div[@class='order-summary']//div[@class='order-id']/table[1]//tr[2]//p/strong");
			if (transactionTime != null)
			{
				transaction.TransactionTime = DateTime.Parse(transactionTime.InnerText.Trim(), CultureInfo.InvariantCulture);
			}

			HtmlNode debitedAmount = doc.DocumentNode.SelectSingleNode("//div[@class='order-content']//table/tbody/tr[last()]/td");
			if (debitedAmount != null)
			{
				int index = Regex.Match(debitedAmount.InnerText, @"\d+").Index;//TODO: Convert to compile time regex
				string amountValue = debitedAmount.InnerText[index..].Trim();
				string currencySymbol = debitedAmount.InnerText[..index].Trim();
				transaction.Debited = double.Parse(amountValue);
				transaction.Currency = currencySymbol;
			}
			transaction.Vendor = vendor;
			return transaction;
		}
	}
}
