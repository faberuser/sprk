using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ShakmehDungeonData : IDataKeySelector<ShakmehDungeonKey>
{
	[Key(0)]
	public int ShakmehIndex { get; set; }

	[Key(1)]
	public int Level { get; set; }

	[Key(2)]
	public int DungeonIndex { get; set; }

	[Key(3)]
	public int[] OpenCondition { get; set; }

	[Key(4)]
	public int FirstClearRewardIndex { get; set; }

	[Key(5)]
	public int RewardIndex { get; set; }

	[Key(6)]
	public int BanRuleIndex { get; set; }

	ShakmehDungeonKey IDataKeySelector<ShakmehDungeonKey>.Key => new ShakmehDungeonKey(ShakmehIndex, Level);
}
