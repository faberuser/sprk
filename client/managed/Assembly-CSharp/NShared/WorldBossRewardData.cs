using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class WorldBossRewardData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int MinRank { get; set; }

	[Key(2)]
	public int MaxRank { get; set; }

	[Key(3)]
	public int Percentage { get; set; }

	[Key(4)]
	public int RewardIndex { get; set; }

	[Key(5)]
	public WorldBossServerRewardType RewardServerBuffType { get; set; }

	[Key(6)]
	public string[] RewardServerBuffValue { get; set; }

	[Key(7)]
	public string RewardServerBuffCode { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "[Index=" + Index + "]";
	}
}
