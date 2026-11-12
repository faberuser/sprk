using NShared.NJit;

namespace NShared;

public class EquipGachaClientDataContainer : JitDataContainerDefault<EquipGachaClientDataKey, EquipGachaClientData>
{
	public const string TABLENAME = "EquipGachaClientTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EquipGachaClientTable");

	public EquipGachaClientData GetData(int index, HighGachaCategoryType categoryType)
	{
		return GetData(new EquipGachaClientDataKey(index, categoryType));
	}
}
