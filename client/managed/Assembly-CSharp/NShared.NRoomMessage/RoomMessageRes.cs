namespace NShared.NRoomMessage;

[MessagePacket]
public class RoomMessageRes : BaseResponse
{
	public RoomMessageResType Result { get; set; }
}
