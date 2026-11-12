using NShared.NJit;

namespace NShared;

public class DungeonPassiveEnvironmentDataContainer : JitDataContainerDefault<int, DungeonPassiveEnvironmentData>
{
	public const string TABLENAME = "DungeonPassiveEnvironmentTable";

	public override TablePathDefine PathDefine => new TablePathDefine("DungeonPassiveEnvironmentTable");
}
