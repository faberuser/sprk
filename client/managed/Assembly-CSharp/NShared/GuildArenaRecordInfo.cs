namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildArenaRecordInfo
{
	public int SessionIndex { get; set; }

	public string LeftServerGroup { get; set; }

	public long LeftGuildId { get; set; }

	public long LeftAccountId { get; set; }

	public string RightServerGroup { get; set; }

	public long RightGuildId { get; set; }

	public long RightAccountId { get; set; }

	public int DeckIndex { get; set; }

	public int Win { get; set; }

	public string UpdatedTime { get; set; }
}
