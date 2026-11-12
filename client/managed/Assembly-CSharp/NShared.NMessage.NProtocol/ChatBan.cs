namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class ChatBan
{
	public int BanHour { get; set; }

	public ChatBanInfo[] Infos { get; set; }
}
