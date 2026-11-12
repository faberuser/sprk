using NShared.NJit;

namespace NShared;

public class MazeInfoDataContainer : JitDataContainerDefault<int, MazeInfoData>
{
	public const string TABLENAME = "MazeInfoTable";

	public override TablePathDefine PathDefine => new TablePathDefine("MazeInfoTable");
}
