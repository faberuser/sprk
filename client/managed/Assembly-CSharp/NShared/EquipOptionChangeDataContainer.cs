using NShared.NJit;

namespace NShared;

public class EquipOptionChangeDataContainer : JitDataContainerDefault<EquipOptionChangeDataKey, EquipOptionChangeData>
{
	public const string TABLENAME = "EquipOptionChangeTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EquipOptionChangeTable");

	public int GetChageOptionIndex(int optionIndex, int beforeTier, int afterTier)
	{
		return GetData(new EquipOptionChangeDataKey(optionIndex, beforeTier, afterTier))?.ResultOptionIndex ?? 0;
	}
}
