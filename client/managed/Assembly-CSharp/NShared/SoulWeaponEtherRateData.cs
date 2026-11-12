using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class SoulWeaponEtherRateData : IDataKeySelector<SoulWeaponEtherRateKey>
{
	[Key(0)]
	public int ReinforceLevel { get; set; }

	[Key(1)]
	public string ItemCode { get; set; }

	[Key(2)]
	public int AddedBonusRate { get; set; }

	SoulWeaponEtherRateKey IDataKeySelector<SoulWeaponEtherRateKey>.Key => new SoulWeaponEtherRateKey(ReinforceLevel, ItemCode);
}
