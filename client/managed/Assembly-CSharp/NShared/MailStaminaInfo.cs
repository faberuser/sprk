namespace NShared;

[JsonSerializable(Javascript = true)]
public class MailStaminaInfo
{
	public CurrencyType StaminaType { get; set; }

	public int Amount { get; set; }
}
