using NShared.NJit;

namespace NShared;

public class EquipAutoDataContainer : JitDataContainerDefault<EquipAutoKey, EquipAutoData>
{
	public const string TABLENAME = "EquipAutoTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EquipAutoTable");

	public EquipAutoData GetData(CreatureTagType creatureTagType, int equipSlotIndex)
	{
		return GetData(new EquipAutoKey(creatureTagType, equipSlotIndex));
	}
}
