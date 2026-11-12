using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EquipOptionUpgradeData : IDataKeySelector<EquipOptionUpgradeDataKey>
{
	EquipOptionUpgradeDataKey IDataKeySelector<EquipOptionUpgradeDataKey>.Key => new EquipOptionUpgradeDataKey(Tier, DetailIndex);

	[Key(0)]
	public short Tier { get; set; }

	[Key(1)]
	public int DetailIndex { get; set; }

	[Key(2)]
	public int ReqGold { get; set; }

	[Key(3)]
	public int MaterialItemIndex { get; set; }

	[Key(4)]
	public int MaterialItemCount { get; set; }

	public override string ToString()
	{
		return "Tier=" + Tier + " DetailIndex=" + DetailIndex;
	}
}
