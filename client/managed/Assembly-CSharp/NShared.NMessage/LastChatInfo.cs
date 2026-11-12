namespace NShared.NMessage;

[JsonSerializable]
public class LastChatInfo
{
	public LastSingleChatInfo LastChannelChatInfo { get; set; }

	public LastSingleChatInfo LastGuildChatInfo { get; set; }

	public LastSingleChatInfo LastPartyChatInfo { get; set; }

	public LastSingleChatInfo[] LastWhisperChatInfos { get; set; }
}
