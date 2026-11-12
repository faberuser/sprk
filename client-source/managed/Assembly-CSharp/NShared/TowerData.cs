using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class TowerData : IDataKeySelector<int>
{
	[Key(0)]
	public int TowerIndex { get; set; }

	[Key(1)]
	public string Name { get; set; }

	[Key(2)]
	public TowerType Type { get; set; }

	[Key(3)]
	public int FloorCountPerPage { get; set; }

	[Key(4)]
	public bool DailyReset { get; set; }

	[Key(5)]
	public bool WeeklyReset { get; set; }

	[Key(6)]
	public int[] MonthlyResetDate { get; set; }

	[Key(7)]
	public bool RepeatableFloor { get; set; }

	[Key(8)]
	public bool UseNpc { get; set; }

	[Key(9)]
	public int MaxResetCount { get; set; }

	[Key(10)]
	public RewardType ResetCostType { get; set; }

	[Key(11)]
	public int[] ResetCost { get; set; }

	[Key(12)]
	public int[] CompleteRewardIndex { get; set; }

	[Key(13)]
	public string[] TowerImage { get; set; }

	[Key(14)]
	public string SkyImage { get; set; }

	int IDataKeySelector<int>.Key => TowerIndex;

	public override string ToString()
	{
		return "TowerData Index=" + TowerIndex;
	}
}
