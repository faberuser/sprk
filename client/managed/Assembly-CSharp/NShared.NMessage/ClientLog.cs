namespace NShared.NMessage;

[MessagePacket]
public class ClientLog : BaseRequest
{
	public string Log { get; set; }
}
