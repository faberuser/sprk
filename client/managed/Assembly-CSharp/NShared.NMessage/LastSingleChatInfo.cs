namespace NShared.NMessage;

[JsonSerializable]
public class LastSingleChatInfo
{
	public long AccountId { get; set; }

	public string ReadTime { get; set; }
}
