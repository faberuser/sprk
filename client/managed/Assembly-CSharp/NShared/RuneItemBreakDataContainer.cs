using NShared.NJit;

namespace NShared;

public class RuneItemBreakDataContainer : JitDataContainerDefault<int, RuneItemBreakData>
{
	public const string TABLENAME = "RuneItemBreakTable";

	public override TablePathDefine PathDefine => new TablePathDefine("RuneItemBreakTable");
}
