namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class CurrencyInfoChanged
{
	public CurrencyType CurrencyType { get; set; }

	public long Amount { get; set; }
}
