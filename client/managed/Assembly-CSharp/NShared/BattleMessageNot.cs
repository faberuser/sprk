using NShared.NMessage;

namespace NShared;

[BattlePacket]
public class BattleMessageNot : IMessageNot
{
	public long SenderId { get; set; }

	public MessageGroupType GroupType { get; set; }

	public long[] ReceiverIds { get; set; }

	public string Type { get; set; }

	public string Content { get; set; }
}
