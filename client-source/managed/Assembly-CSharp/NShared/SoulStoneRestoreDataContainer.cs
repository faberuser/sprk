using NShared.NJit;

namespace NShared;

public class SoulStoneRestoreDataContainer : JitDataContainerDefault<string, SoulStoneRestoreData>
{
	public const string TABLENAME = "SoulStoneRestoreTable";

	public override TablePathDefine PathDefine => new TablePathDefine("SoulStoneRestoreTable");
}
