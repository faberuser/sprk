namespace NShared.NRoomMessage;

[MessagePacket]
public class RoomLoginReq : BaseRequest
{
	public long AccountId { get; set; }

	public long RoomId { get; set; }
}
