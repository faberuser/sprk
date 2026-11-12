namespace NShared.NRoomMessage.NProtocol;

[JsonSerializable]
public class RoomJoinNotice
{
	public CachedUserInfo UserInfo { get; set; }
}
