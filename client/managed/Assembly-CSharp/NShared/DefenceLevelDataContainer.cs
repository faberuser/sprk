using NShared.NJit;

namespace NShared;

public class DefenceLevelDataContainer : JitDataContainerDefault<int, DefenceLevelData>
{
	public const string TABLENAME = "DefenceLevelTable";

	public override TablePathDefine PathDefine => new TablePathDefine("DefenceLevelTable");
}
