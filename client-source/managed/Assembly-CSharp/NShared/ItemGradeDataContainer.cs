using NShared.NJit;

namespace NShared;

public class ItemGradeDataContainer : JitDataContainerDefault<int, ItemGradeData>
{
	public const string TABLENAME = "ItemGradeTable";

	public override TablePathDefine PathDefine => new TablePathDefine("ItemGradeTable");
}
