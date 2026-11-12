using NShared.NJit;

namespace NShared;

public class WantedPathDataContainer : JitDataContainerDefault<int, WantedPathData>
{
	public const string TABLENAME = "WantedPathTable";

	public override TablePathDefine PathDefine => new TablePathDefine("WantedPathTable");
}
