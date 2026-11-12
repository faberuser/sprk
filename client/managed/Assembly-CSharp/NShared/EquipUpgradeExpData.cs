using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EquipUpgradeExpData : IDataKeySelector<EquipUpgradeExpDataKey>
{
	EquipUpgradeExpDataKey IDataKeySelector<EquipUpgradeExpDataKey>.Key => new EquipUpgradeExpDataKey(PartType, Tier, DetailIndex, Level);

	[Key(0)]
	public EquipPartType PartType { get; set; }

	[Key(1)]
	public short Tier { get; set; }

	[Key(2)]
	public int DetailIndex { get; set; }

	[Key(3)]
	public short Level { get; set; }

	[Key(4)]
	public int ReqExp { get; set; }

	[Key(5)]
	public int GiveExp { get; set; }

	public override string ToString()
	{
		return "PartType=" + PartType.ToString() + " Tier=" + Tier + " DetailIndex=" + DetailIndex + " Level=" + Level;
	}
}
