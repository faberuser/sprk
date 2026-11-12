namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class SetUserNick
{
	public long AccountId { get; set; }

	public string Nick { get; set; }
}
