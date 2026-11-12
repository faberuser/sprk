using NShared.NJit;

namespace NShared;

public class CutSceneForegroundDataContainer : JitDataContainerDefault<int, CutSceneForegroundData>
{
	public const string TABLENAME = "CutSceneForegroundTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CutSceneForegroundTable");
}
