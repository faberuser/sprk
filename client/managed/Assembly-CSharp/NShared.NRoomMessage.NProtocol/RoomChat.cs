namespace NShared.NRoomMessage.NProtocol;

[JsonSerializable]
public class RoomChat
{
	public bool IsSenderMaster { get; set; }

	public int SenderAvatarIndex { get; set; }

	public int SenderTeamLevel { get; set; }

	public string SenderName { get; set; }

	public string Chat { get; set; }

	public string SendTime { get; set; }
}
