namespace NShared.NRoomMessage.NProtocol;

[JsonSerializable]
public class RoomInfo
{
	public byte Opened { get; set; }

	public string RoomName { get; set; }
}
