using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class KarmaGaugeExponentialData : IDataKeySelector<KarmaGaugeExponentialKey>
{
	[Key(0)]
	public int PunishmentGroupIndex { get; set; }

	[Key(1)]
	public int Wave { get; set; }

	[Key(2)]
	public float ExponentialValue { get; set; }

	KarmaGaugeExponentialKey IDataKeySelector<KarmaGaugeExponentialKey>.Key => new KarmaGaugeExponentialKey(PunishmentGroupIndex, Wave);
}
