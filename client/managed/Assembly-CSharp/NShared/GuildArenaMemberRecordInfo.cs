namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildArenaMemberRecordInfo
{
	public GuildMemberServerInfo LeftTeam { get; set; }

	public GuildMemberServerInfo RightTeam { get; set; }

	public int DeckIndex { get; set; }

	public int Win { get; set; }

	public string UpdatedTime { get; set; }
}
