namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class GuildRaidEnd
{
	public string Nick { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public GuildRaidDungeonInfo ResultGuildRaidInfo { get; set; }

	public bool Completed { get; set; }
}
