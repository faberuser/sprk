using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EquipGachaClientData : IDataKeySelector<EquipGachaClientDataKey>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public HighGachaCategoryType HighGachaCategoryType { get; set; }

	[Key(2)]
	public int[] UniqueWeaponHeroIndex { get; set; }

	EquipGachaClientDataKey IDataKeySelector<EquipGachaClientDataKey>.Key => new EquipGachaClientDataKey(Index, HighGachaCategoryType);

	public override string ToString()
	{
		return "Index=" + Index + "HighGachaCategoryType" + HighGachaCategoryType;
	}
}
