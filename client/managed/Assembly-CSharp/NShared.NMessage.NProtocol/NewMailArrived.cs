namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class NewMailArrived
{
	public long MailIndex { get; set; }

	public bool IsGlobal { get; set; }
}
