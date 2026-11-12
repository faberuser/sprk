namespace NShared;

[JsonSerializable(Javascript = true)]
public class MailCurrencyInfo
{
	public CurrencyType CurrencyType { get; set; }

	public int Amount { get; set; }
}
