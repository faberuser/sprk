namespace NShared.NRoomMessage;

[MessagePacket]
public class RoomPingRes : BaseResponse
{
	public RoomPingResType Result { get; set; }
}
