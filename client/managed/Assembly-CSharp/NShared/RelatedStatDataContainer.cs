using NShared.NJit;

namespace NShared;

public class RelatedStatDataContainer : JitDataContainerDefault<StatType, RelatedStatData>
{
	public const string TABLENAME = "RelatedStatTable";

	public override TablePathDefine PathDefine => new TablePathDefine("RelatedStatTable");

	public bool IsContainRelatedDefenceStatGroup(StatType statType)
	{
		return GetData(statType) != null;
	}

	public RelatedStatData GetRelatedDefenceStatType(StatType statType)
	{
		if (IsContainRelatedDefenceStatGroup(statType))
		{
			return GetData(statType);
		}
		return null;
	}
}
