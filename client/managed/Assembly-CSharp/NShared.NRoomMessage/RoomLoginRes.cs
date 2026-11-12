namespace NShared.NRoomMessage;

[MessagePacket]
public class RoomLoginRes : BaseResponse
{
	public RoomLoginResType Result { get; set; }
}
