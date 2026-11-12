namespace NShared.NRoomMessage;

[MessagePacket]
public class RoomMessageReq : BaseRequest
{
	public string Message { get; set; }
}
