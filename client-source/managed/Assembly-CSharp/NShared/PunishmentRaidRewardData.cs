using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PunishmentRaidRewardData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int RaidIndex { get; set; }

	[Key(2)]
	public int RaidLevel { get; set; }

	[Key(3)]
	public PunishmentRaidRewardType RewardType { get; set; }

	[Key(4)]
	public int[] ClearCheckIndex { get; set; }

	[Key(5)]
	public int[] RewardIndex { get; set; }

	[Key(6)]
	public bool Active { get; set; }

	[Key(7)]
	public int InfinityRewardGroupIndex { get; set; }

	[Key(8)]
	public string[] ShowRecieveSubRewardCondition { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "PunishmentRaidRewardData Index=" + Index;
	}
}
