namespace NShared.NMessage;

[MessagePacket]
public class ChangeGuildReq : BaseRequest
{
	public long GuildId { get; set; }
}
