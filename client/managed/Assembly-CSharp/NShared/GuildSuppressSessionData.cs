using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class GuildSuppressSessionData : IDataKeySelector<int>
{
	[Key(0)]
	public int SessionIndex { get; set; }

	[Key(1)]
	public int SeasonIndex { get; set; }

	[Key(2)]
	public string GlobalApplyStart { get; set; }

	[Key(3)]
	public string GlobalApplyEnd { get; set; }

	[Key(4)]
	public string GlobalBattleStart { get; set; }

	[Key(5)]
	public string GlobalBattleEnd { get; set; }

	[Key(6)]
	public int ViewRankSeason { get; set; }

	[Key(7)]
	public int ViewSession { get; set; }

	[Key(8)]
	public int[] SuppressDungeonIndex { get; set; }

	[Key(9)]
	public int RaidIndex { get; set; }

	[Key(10)]
	public int[] Level { get; set; }

	[Key(11)]
	public int[] SingleLevel { get; set; }

	[Key(12)]
	public int PortalViewSession { get; set; }

	int IDataKeySelector<int>.Key => SessionIndex;

	public override string ToString()
	{
		return "GuildSuppressSessionDataIndex=" + SessionIndex;
	}
}
