using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class KarmaRewardData : IDataKeySelector<KarmaRewardKey>
{
	[Key(0)]
	public int PunishmentGroupIndex { get; set; }

	[Key(1)]
	public int RewardIndex { get; set; }

	[Key(2)]
	public bool IsAccumulate { get; set; }

	[Key(3)]
	public float DropChance { get; set; }

	KarmaRewardKey IDataKeySelector<KarmaRewardKey>.Key => new KarmaRewardKey(PunishmentGroupIndex, RewardIndex);
}
