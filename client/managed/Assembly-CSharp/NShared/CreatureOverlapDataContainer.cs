using NShared.NJit;

namespace NShared;

public class CreatureOverlapDataContainer : JitDataContainerDefault<int, CreatureOverlapData>
{
	public const string TABLENAME = "CreatureOverlapTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CreatureOverlapTable");
}
