namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class PartyMemberEmoti
{
	public ChatType ChatType { get; set; }

	public EmotiType Emoti { get; set; }

	public string SenderName { get; set; }

	public int SenderAvatarIndex { get; set; }

	public int SenderTeamLevel { get; set; }

	public long ReceiverId { get; set; }

	public string ReceiverName { get; set; }
}
