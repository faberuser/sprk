namespace NShared.NMessage;

[MessagePacket]
public class LoginRes : BaseResponse
{
	public LoginResType Result { get; set; }

	public MessageNot[] WhisperChats { get; set; }

	public MessageNot[] GuildChats { get; set; }

	public int ChannelNo { get; set; }
}
