namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class GuildRaidBegin
{
	public string Nick { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }
}
