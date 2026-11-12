using System.Linq;
using NShared.NJit;

namespace NShared;

public class AccessoryCostumeGroupDataContainer : JitDataContainerDefault<int, AccessoryCostumeGroupData>
{
	public const string TABLENAME = "AccessoryCostumeCategoryTable";

	public override TablePathDefine PathDefine => new TablePathDefine("AccessoryCostumeCategoryTable");

	public AccessoryCostumeGroupData GetDataByGroupName(string group)
	{
		return GetAllData().First((AccessoryCostumeGroupData groupData) => groupData.CategoryName == group);
	}
}
