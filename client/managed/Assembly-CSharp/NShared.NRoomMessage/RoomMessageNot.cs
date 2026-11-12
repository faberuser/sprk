using NShared.NMessage;

namespace NShared.NRoomMessage;

[MessagePacket]
public class RoomMessageNot : IMessageNot
{
	public long SenderId { get; set; }

	public string Type { get; set; }

	public string Content { get; set; }
}
