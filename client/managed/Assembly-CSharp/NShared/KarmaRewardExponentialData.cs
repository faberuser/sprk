using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class KarmaRewardExponentialData : IDataKeySelector<KarmaRewardExponentialKey>
{
	[Key(0)]
	public int PunishmentGroupIndex { get; set; }

	[Key(1)]
	public int Level { get; set; }

	[Key(2)]
	public int RewardIndex { get; set; }

	[Key(3)]
	public float ExponentialValue { get; set; }

	KarmaRewardExponentialKey IDataKeySelector<KarmaRewardExponentialKey>.Key => new KarmaRewardExponentialKey(PunishmentGroupIndex, Level);
}
