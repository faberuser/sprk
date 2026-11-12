namespace NShared;

[JsonSerializable(Javascript = true)]
public class ChatBanInfo
{
	public ChatBanType BanType { get; set; }

	public ChatBanReason BanReason { get; set; }

	public string EndTime { get; set; }
}
