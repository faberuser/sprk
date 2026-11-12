using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EquipUpgradePriceData : IDataKeySelector<EquipUpgradePriceDataKey>
{
	EquipUpgradePriceDataKey IDataKeySelector<EquipUpgradePriceDataKey>.Key => new EquipUpgradePriceDataKey(Tier, DetailIndex, Level);

	[Key(0)]
	public short Tier { get; set; }

	[Key(1)]
	public int DetailIndex { get; set; }

	[Key(2)]
	public short Level { get; set; }

	[Key(3)]
	public int ReqGold { get; set; }

	public override string ToString()
	{
		return "Tier=" + Tier + " DetailIndex=" + DetailIndex + " Level=" + Level;
	}
}
