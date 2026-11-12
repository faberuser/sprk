namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildArenaRankerRecordInfo
{
	public int SessionIndex { get; set; }

	public int SeasonIndex { get; set; }

	public GuildArenaRankerInfo LeftRanker { get; set; }

	public GuildArenaRankerInfo RightRanker { get; set; }

	public int Win { get; set; }

	public int SessionNumber { get; set; }
}
