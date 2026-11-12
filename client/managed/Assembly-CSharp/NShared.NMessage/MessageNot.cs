namespace NShared.NMessage;

[MessagePacket]
public class MessageNot : IMessageNot
{
	public long SenderId { get; set; }

	public MessageGroupType GroupType { get; set; }

	public long[] ReceiverIds { get; set; }

	public string Type { get; set; }

	public string Content { get; set; }
}
