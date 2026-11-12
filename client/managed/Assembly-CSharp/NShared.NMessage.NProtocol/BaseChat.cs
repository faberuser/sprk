namespace NShared.NMessage.NProtocol;

public class BaseChat
{
	public int SenderAvatarIndex { get; set; }

	public int SenderTeamLevel { get; set; }

	public string SenderName { get; set; }

	public long ReceiverId { get; set; }

	public string ReceiverName { get; set; }

	public string Chat { get; set; }

	public string SendTime { get; set; }

	public int AdminLevel { get; set; }

	public LinkedItemInfo[] LinkedItem { get; set; }

	public int EmoticonIndex { get; set; }

	public bool IsShowBattleEmoti { get; set; }

	public int[] ChatIconList { get; set; }
}
