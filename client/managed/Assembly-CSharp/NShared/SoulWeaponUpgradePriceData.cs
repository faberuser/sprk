using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class SoulWeaponUpgradePriceData : IDataKeySelector<SoulWeaponUpgradePriceKey>
{
	SoulWeaponUpgradePriceKey IDataKeySelector<SoulWeaponUpgradePriceKey>.Key => new SoulWeaponUpgradePriceKey(ItemIndex, Grade);

	[Key(0)]
	public int ItemIndex { get; set; }

	[Key(1)]
	public int Grade { get; set; }

	[Key(2)]
	public string MaterialItemCode { get; set; }

	[Key(3)]
	public int MaterialItemCount { get; set; }

	[Key(4)]
	public int ReqGold { get; set; }
}
